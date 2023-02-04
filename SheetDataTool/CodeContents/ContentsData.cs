using System.Reflection;

namespace SheetDataTool
{
	public class ContentsData
	{
		private readonly List<CodeContents> _contents = new();

		public ContentsData(SheetInfo sheetInfo, Setting setting)
		{
			var constructorParameterTypes = new[] { typeof(SheetInfoView), typeof(Setting) };
			var contentsInfos = TypeUtil.GetChildClasses<CodeContents>()
				.Select(x => (Type : x, Description : x.GetCustomAttribute<ContentsDescriptionAttribute>() ?? throw new Exception($"{x.FullName} must have {nameof(ContentsDescriptionAttribute)}.")))
				.ToDictionary(x => x.Description.Name,
					x => new { x.Description.CanRegisterMultiple, ConstructorInfo = x.Type.GetConstructor(constructorParameterTypes) ?? throw new Exception($"{x.Type.FullName} must have constructor with ({nameof(SheetInfoView)}, {nameof(Setting)}) parameter.")});
			
			var addedContentNames = new HashSet<string>();

			for(var row = 0; row < sheetInfo.RowCount;) 
			{
				var firstCell = sheetInfo[row, 0];
				if (CodeContents.IsContentsCell(firstCell) is false)
				{
					++row;
					continue;
				}
				
				var contentsName = CodeContents.GetContentsName(firstCell)?.ChangeNotation(setting.InputNotation, Notation.Pascal);
				if (contentsName is null or "Name" or "Data")
				{
					break;
				}
				
				var endRow = sheetInfo.FindRow(row + 1, 0, x => string.IsNullOrWhiteSpace(x) || CodeContents.IsContentsCell(x));
				if (endRow == -1) endRow = sheetInfo.RowCount;
				var sheetInfoView = new SheetInfoView(sheetInfo, row, 0, endRow - 1, sheetInfo.ColumnCount - 1);

				if(contentsInfos.TryGetValue(contentsName, out var contentsInfo))
				{
					var contents = (contentsInfo.ConstructorInfo.Invoke(BindingFlags.DoNotWrapExceptions, null, new object[] { sheetInfoView, setting }, null) as CodeContents)!;

					if (contentsInfo.CanRegisterMultiple is false)
					{
						if (addedContentNames.Add(contentsName) is false)
						{
							throw new InvalidSheetRuleException($"'{contentsName}' is content that cannot be used multiple times.", row, 0);
						}
					}
					_contents.Add(contents);
				}
				else
				{
					throw new InvalidSheetRuleException($"{contentsName} is an invalid contents name.", row, 0);
				}

				row = endRow;
			}
		}

		public override string ToString()
		{
			return string.Join('\n', _contents);
		}
	}
}
