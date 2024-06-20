using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SheetDataTool
{
	public class ExcelSheetUtil : SheetUtil
	{
		private readonly string _path;
		private readonly Dictionary<string, string> _pathBySheetName = new();

		public ExcelSheetUtil(string path)
		{
			_path = path;
			RefreshSheetList();
		}

		public IEnumerable<string> GetSheetNames() => _pathBySheetName.Keys;

		public SheetInfo GetSheetInfo(string sheetName)
		{
			if (_pathBySheetName.TryGetValue(sheetName, out var path) is false)
			{
				throw new InvalidSheetNameOrPathException(sheetName, null);
			}

			if (File.Exists(path) is false)
			{
				throw new InvalidSheetNameOrPathException(sheetName, path);
			}

			using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var document = SpreadsheetDocument.Open(stream, false);
			var workbookPart = document.WorkbookPart;
			if (workbookPart is null)
			{
				throw new InvalidSheetNameOrPathException(sheetName, path);
			}

			var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == sheetName);
			if (sheet?.Id is null)
			{
				throw new InvalidSheetNameOrPathException(sheetName, path);
			}

			if (workbookPart.GetPartById(sheet.Id!) is not WorksheetPart worksheetPart)
			{
				throw new InvalidSheetNameOrPathException(sheetName, path);
			}

			var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>()
				.FirstOrDefault()?.SharedStringTable.ToList();

			var worksheet = worksheetPart.Worksheet;
			var sheetLastReference =
				ReferenceUtil.ParseReference(worksheet.SheetDimension!.Reference!.Value!.Split(':').Last());
			var cellData = new string[sheetLastReference.rowIndex + 1, sheetLastReference.columnIndex + 1];

			var sheetData = worksheet.Elements<SheetData>().First();
			foreach (var row in sheetData.Elements<Row>())
			{
				foreach (var cell in row.Elements<Cell>())
				{
					var value = cell.InnerText;
					if (cell.InnerText.Length > 0)
					{
						if (cell.DataType is null)
						{
							if (cell.CellFormula is not null)
							{
								value = cell.CellValue?.InnerText ?? string.Empty;
							}
						}
						else
						{
							switch (cell.DataType.Value)
							{
								case CellValues.SharedString:
									if (stringTable is not null)
									{
										value = stringTable[int.Parse(value)].InnerText;
									}
									else
									{
										throw new Exception("SharedString does not exist.");
									}

									break;

								case CellValues.Boolean:
									value = value switch
									{
										"0" => "FALSE",
										_ => "TRUE"
									};
									break;

								case CellValues.Number:
								case CellValues.Error:
								case CellValues.String:
								case CellValues.InlineString:
								case CellValues.Date:
									break;

								default:
									throw new ArgumentOutOfRangeException();
							}
						}
					}

					var reference = ReferenceUtil.ParseReference(cell.CellReference!.Value!);
					cellData[reference.rowIndex, reference.columnIndex] = string.IsNullOrEmpty(value) ? null : value;
				}
			}

			return new SheetInfo(sheetName, cellData);
		}

		public void RefreshSheetList()
		{
			_pathBySheetName.Clear();

			if (Directory.Exists(_path) is false)
			{
				throw new InvalidDirectoryException(_path);
			}

			var fileNames = Directory.GetFiles(_path, "*.xlsx");

			var overlapInfos = new List<OverlapSheetNameException.OverlapInfo>();
			foreach (var fileName in fileNames)
			{
				var fileInfo = new FileInfo(fileName);
				if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
				{
					continue;
				}

				using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using var document = SpreadsheetDocument.Open(stream, false);
				var wbPart = document.WorkbookPart;
				var sheets = wbPart?.Workbook.Sheets;
				if (sheets is null) continue;

				var sheetNames = sheets.Cast<Sheet>().Where(x => x.Name?.Value is not null).Select(x => x.Name?.Value!)
					.ToList();
				sheetNames.ForEach(sheetName =>
				{
					if (_pathBySheetName.TryAdd(sheetName, fileName) is false)
					{
						overlapInfos.Add(new OverlapSheetNameException.OverlapInfo
							{ SheetName = sheetName, SheetPath = fileName });
					}
				});
			}

			if (overlapInfos.Any())
			{
				throw new OverlapSheetNameException(overlapInfos);
			}
		}

		public string GetPath(string sheetName)
		{
			_pathBySheetName.TryGetValue(sheetName, out var path);
			return path;
		}
	}
}
