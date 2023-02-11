using System.Reflection;

namespace SheetDataTool
{
	public sealed class ContentsData
	{
		private readonly List<CodeContents> _contents = new();
		private readonly SheetInfo _sheetInfo;
		private readonly Setting _setting;

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

			_sheetInfo = sheetInfo;
			_setting = setting;
		}

		public string GetScript()
		{
			var sb = new ScopedStringBuilder();

			void WriteContents<T>( bool isGlobal ) where T : CodeContents
				=> _contents.OfType<T>().ForEach(x => x.WriteScript(sb, isGlobal, _setting));
			
			var designContents = _contents.OfType<DesignContents>().FirstOrDefault();
			var sheetName = _sheetInfo.Name.ChangeNotation(_setting.InputNotation, _setting.ScriptDesignNameNotation);
			var keyTypeName = designContents?.KeyType;

			WriteUsingNamespaces(sb);

			var namespaceScope = string.IsNullOrWhiteSpace(_setting.NamespaceName)
				? null : sb.StartScope($"namespace {_setting.NamespaceName}");

			WriteContents<EnumContents>(true);
			WriteContents<RecordContents>(true);
			WriteContents<InterfaceContents>(true);

			var hasDesignContents = designContents is not null;
			if (hasDesignContents)
			{
				WriteContents<DescriptionContents>(true);

				var inheritedInterfaceNames = designContents?.InheritedInterfaceNames
					?.Select(x => $"{_setting.ScriptInterfaceNamePrefix}{x.ChangeNotation(_setting.InputNotation, _setting.ScriptInterfaceNameNotation)}")
					.Aggregate((x, y) => $"{x}, {y}");
				var declare = $"public partial record {sheetName} : SheetData<{keyTypeName}, {sheetName}>, ISheetData<{keyTypeName}>{(inheritedInterfaceNames is null ? "" : $", {inheritedInterfaceNames}")}";
				using (sb.StartScope(declare))
				{
					WriteContents<EnumContents>(false);
					WriteContents<RecordContents>(false);
					WriteContents<ConstantContents>(false);
					WriteContents<DesignContents>(false);
				}
			}
			namespaceScope?.Dispose();
			return sb.ToString();
		}

		private static void WriteUsingNamespaces( ScopedStringBuilder sb )
		{
			var items = new List<string>
			{
				"System",
				"System.Collections",
				"System.Collections.Generic",
				"Newtonsoft.Json",
				"UnityEngine"
			};
			items.ForEach(x => sb.WriteLine($"using {x};"));
			sb.WriteLine();
		}

		public override string ToString()
		{
			return string.Join('\n', _contents);
		}
	}
}
