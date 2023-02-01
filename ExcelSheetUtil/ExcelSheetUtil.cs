using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SheetDataTool;

namespace ExcelSheetUtil
{
	public static class ExcelSheetUtil
	{
		public static IEnumerable<string> GetSheetNames(string path)
		{
			using var document = SpreadsheetDocument.Open(path, false);
			var wbPart = document.WorkbookPart;
			if (wbPart is null)
			{
				return Enumerable.Empty<string>();
			}

			var sheets = wbPart.Workbook.Sheets;
			return sheets is null
				? Enumerable.Empty<string>()
				: sheets.Cast<Sheet>().Select(x => x.Name?.Value ?? string.Empty);
		}

		public static SheetInfo GetSheetInfo(string path, string sheetName)
		{
			using var document = SpreadsheetDocument.Open(path, false);
			var workbookPart = document.WorkbookPart;
			if (workbookPart is null)
			{
				throw new Exception("WorkbookPart is null.");
			}

			var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == sheetName);
			if (sheet?.Id is null) 
			{
				throw new Exception($"Sheet does not exist!(Name : {sheetName})");
			}

			if (workbookPart.GetPartById(sheet.Id!) is not WorksheetPart worksheetPart)
			{
				throw new Exception($"WorkSheetPart is null.(Name : {sheetName})");
			}

			var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>()
				.FirstOrDefault()?.SharedStringTable.ToList();

			var worksheet = worksheetPart.Worksheet;
			var sheetLastReference = ParseReference(worksheet.SheetDimension!.Reference!.Value!.Split(':').Last());
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

					var reference = ParseReference(cell.CellReference!.Value!);
					cellData[reference.rowIndex, reference.columnIndex] = value;
				}
			}

			return new SheetInfo(sheetName, cellData);
		}

		private static (int rowIndex, int columnIndex) ParseReference( string text ) 
		{
			if (text == null) 
			{
				throw new ArgumentNullException(nameof(text));
			}

			var match = Regex.Match(text, @"([a-zA-Z]+)(\d+)");
			if (!match.Success) 
			{
				throw new FormatException(nameof(text));
			}

			var columnText = match.Groups[1].Value;
			var rowText = match.Groups[2].Value;
			var row = int.Parse(rowText);
			const int alphabetCount = 'Z' - 'A' + 1;
			var column = columnText.Aggregate(0, ( current, t ) => ( current * alphabetCount ) + ( (int)t - 'A' + 1 ));
			return (row - 1, column - 1);
		}
	}
}