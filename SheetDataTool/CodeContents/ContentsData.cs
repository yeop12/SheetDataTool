using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SheetDataTool
{
	public sealed class ContentsData
	{
		private enum ScriptType
		{
			None,
			Constant,
			Design,
			Full,
		}

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

		private bool HasContents<T>() where T : CodeContents => _contents.OfType<T>().FirstOrDefault() is not null;

		public string GetScript(bool madeForSerialization)
		{
			var sb = new ScopedStringBuilder();

			void WriteContents<T>( bool isGlobal ) where T : CodeContents
				=> _contents.OfType<T>().ForEach(x => x.WriteScript(sb, isGlobal, _setting, madeForSerialization));
			
			var designContents = _contents.OfType<DesignContents>().FirstOrDefault();
			var constantContents = _contents.OfType<ConstantContents>().FirstOrDefault();
			var keyTypeName = designContents?.KeyType;
			var sheetName = _sheetInfo.Name.ChangeNotation(_setting.InputNotation, _setting.ScriptClassNameNotation);

			WriteUsingNamespaces(sb);

			var namespaceScope = string.IsNullOrWhiteSpace(_setting.NamespaceName)
				? null : sb.StartScope($"namespace {_setting.NamespaceName}");

			WriteContents<EnumContents>(true);
			WriteContents<RecordContents>(true);
			WriteContents<InterfaceContents>(true);

			var scriptType = GetScriptType(constantContents is not null, designContents is not null);
			if (scriptType != ScriptType.None)
			{
				WriteContents<DescriptionContents>(true);

				var inheritedInterfaceNames = designContents?.InheritedInterfaceNames
					?.Select(x => $"{_setting.ScriptInterfaceNamePrefix}{x.ChangeNotation(_setting.InputNotation, _setting.ScriptInterfaceNameNotation)}")
					.Aggregate((x, y) => $"{x}, {y}");

				var declare = scriptType switch
				{
					ScriptType.Full => $"public sealed partial record {sheetName} : {ScriptUtil.GetFullClassName(_setting)}<{keyTypeName}, {sheetName}>, {ScriptUtil.GetDesignInterfaceName(_setting)}<{keyTypeName}>{( inheritedInterfaceNames is null ? "" : $", {inheritedInterfaceNames}" )}",
					ScriptType.Design => $"public sealed partial record {sheetName} : {ScriptUtil.GetDesignClassName(_setting)}<{keyTypeName}, {sheetName}>, {ScriptUtil.GetDesignInterfaceName(_setting)}<{keyTypeName}>{( inheritedInterfaceNames is null ? "" : $", {inheritedInterfaceNames}" )}",
					ScriptType.Constant => $"public sealed partial record {sheetName} : {ScriptUtil.GetConstantClassName(_setting)}<{sheetName}>",
					_ => throw new NotImplementedException($"{scriptType}")
				};

				using (sb.StartScope(declare))
				{
					if (madeForSerialization)
					{
						sb.WriteLine("private static bool _serializeDesign { get; set; }");
					}
					WriteContents<EnumContents>(false);
					WriteContents<RecordContents>(false);
					WriteContents<ConstantContents>(false);
					WriteContents<DesignContents>(false);
				}
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		private void WriteUsingNamespaces( ScopedStringBuilder sb )
		{
			var basicItems = new List<string>
			{
				"System",
				"System.Collections",
				"System.Collections.Generic",
				"Newtonsoft.Json",
				"Newtonsoft.Json.Converters",
			};
			basicItems.ForEach(x => sb.WriteLine($"using {x};"));
			var unityItems = new List<string>
			{
				"UnityEngine",
			};
			sb.WriteLine($"#if {_setting.UnityPlatformDefine}");
			unityItems.ForEach(x => sb.WriteLine($"using {x};"));
			sb.WriteLine("#endif");
			sb.WriteLine();
		}

		private ScriptType GetScriptType(bool hasConstant, bool hasDesign)
		{
			if (hasConstant && hasDesign) return ScriptType.Full;
			if (hasConstant) return ScriptType.Constant;
			if(hasDesign) return ScriptType.Design;
			return ScriptType.None;
		}

		private abstract class AccessInfo
		{
			public abstract Type OutputType { get; protected init; }

			protected AccessInfo(string value, Type parentType, Setting setting)
			{
			}

			public abstract void SetValue(object target, object? value);

			public abstract object? GetValue(object target);

			public static AccessInfo Generate(string value, Type parentType, Setting setting)
			{
				var isList = parentType.IsGenericType && parentType.GetGenericTypeDefinition() == typeof(List<>);
				if (isList) return new ListAccessInfo(value, parentType, setting);
				return new PropertyAccessInfo(value, parentType, setting);
			}
		}

		private sealed class ListAccessInfo : AccessInfo
		{
			private readonly int _listIndex;
			public override Type OutputType { get; protected init; }

			public ListAccessInfo(string value, Type parentType, Setting setting) : base(value, parentType, setting)
			{
				if (int.TryParse(value, out var index) is false) throw new Exception($"List must be integer value.");
				_listIndex = index;
				OutputType = parentType.GetGenericArguments()[0];
			}

			public override void SetValue(object target, object? value)
			{
				if (target is not IList list) throw new Exception("Target is not a list.");
				if (list.Count <= _listIndex)
				{
					var addCount = _listIndex - list.Count + 1;
					for (var i = 0; i < addCount; ++i)
					{
						var item = Activator.CreateInstance(OutputType);
						list.Add(item);
					}
				}

				list[_listIndex] = value;
			}

			public override object? GetValue(object target)
			{
				if (target is not IList list) throw new Exception("Target is not a list.");
				return _listIndex >= list.Count ? null : list[_listIndex];
			}
		}

		private sealed class PropertyAccessInfo : AccessInfo
		{
			private readonly PropertyInfo _propertyInfo;
			public override Type OutputType { get; protected init; }

			public PropertyAccessInfo(string value, Type parentType, Setting setting) : base(value, parentType, setting)
			{
				var propertyName = TypeUtil.UnityTypeNames.Contains(parentType.Name)
					? value
					: value.ChangeNotation(setting.InputNotation,
						setting.ScriptPublicVariableNameNotation);
				_propertyInfo = parentType.GetProperty(propertyName) ?? throw new Exception("Property does not exist.");
				if (_propertyInfo.PropertyType.IsGenericType &&
				    _propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
				{
					propertyName = @$"{setting.ScriptPrivateVariableNamePrefix}{value.ChangeNotation(setting.InputNotation,
						setting.ScriptPrivateVariableNameNotation)}";
					_propertyInfo = parentType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Property does not exist.");
				}
				
				OutputType = _propertyInfo.PropertyType;
			}

			public override void SetValue(object target, object? value)
			{
				_propertyInfo.SetValue(target, value);
			}

			public override object? GetValue(object target)
			{
				return _propertyInfo.GetValue(target);
			}
		}

		private object GetDesignObject( Assembly assembly, string sheetTypeName, Type sheetType )
		{
			var nameRow =  _sheetInfo.FindRow(0, 0, x => CodeContents.IsContentsCell(x) && CodeContents.GetContentsName(x) == "Name".ChangeNotation(Notation.Pascal, _setting.InputNotation));
			if (nameRow == -1)
			{
				throw new InvalidSheetRuleException("Name contents does not exist.");
			}

			if (nameRow == _sheetInfo.RowCount - 1)
			{
				throw new InvalidSheetRuleException("Name contents does not contain items.", nameRow, 0);
			}
			
			var accessInfosByColumn = new Dictionary<int, List<AccessInfo>>();

			var nameSeparators = new[] { '.', '[', ']' };
			for (var column = 0; column < _sheetInfo.ColumnCount; ++column)
			{
				var cell = _sheetInfo[nameRow + 1, column];
				if (_setting.IsIgnoreCell(cell)) continue;

				var properties = new List<AccessInfo>();

				var names = cell.Split(nameSeparators, StringSplitOptions.RemoveEmptyEntries);
				var previousType = sheetType;
				foreach (var name in names)
				{
					try
					{
						var accessInfo = AccessInfo.Generate(name, previousType, _setting);
						previousType = accessInfo.OutputType;
						properties.Add(accessInfo);
					}
					catch (Exception e)
					{
						throw new InvalidSheetRuleException(e.Message, nameRow + 1, column);
					}
				}

				accessInfosByColumn.Add(column, properties);
			}

			var dataRow =  _sheetInfo.FindRow(nameRow+2, 0, x => CodeContents.IsContentsCell(x) && CodeContents.GetContentsName(x) == "Data".ChangeNotation(Notation.Pascal, _setting.InputNotation));
			if (dataRow == -1)
			{
				throw new InvalidSheetRuleException("Data contents does not exist.");
			}
			if (dataRow == _sheetInfo.RowCount - 1)
			{
				throw new InvalidSheetRuleException("Data contents does not contain items.", nameRow, 0);
			}

			var items = (Activator.CreateInstance(typeof(List<>).MakeGenericType(sheetType)) as IList)!;

			for (var row = dataRow + 1; row < _sheetInfo.RowCount; ++row)
			{
				if (_setting.IsIgnoreCell(_sheetInfo[row, 0])) continue;
				var item = Activator.CreateInstance(sheetType) ?? throw new Exception("Type does not exist.");
				foreach (var (column, accessInfos) in accessInfosByColumn)
				{
					var target = item;
					var cell = _sheetInfo[row, column];
					if (cell is null)
					{
						var isListItem = accessInfos.Any(x =>
							x.OutputType.IsGenericType && x.OutputType.GetGenericTypeDefinition() == typeof(List<>));
						if (isListItem) continue;
						var lastType = accessInfos.Last().OutputType;
						if (lastType.IsGenericType && lastType.GetGenericTypeDefinition() == typeof(Nullable<>)) continue;
						throw new InvalidSheetRuleException(
							"Cell cannot be empty except when it is a List item or a Nullable type.", row, column);
					}

					for (var i = 0; i < accessInfos.Count; ++i)
					{
						var accessInfo = accessInfos[i];
						if (i == accessInfos.Count - 1)
						{
							var value = TypeUtil.ChangeType(cell, accessInfo.OutputType);
							accessInfo.SetValue(target, value);
						}
						else
						{
							var newTarget = accessInfo.GetValue(target);
							if (newTarget is null)
							{
								newTarget = Activator.CreateInstance(accessInfo.OutputType) ?? throw new Exception("Type does not exist.");
								accessInfo.SetValue(target, newTarget);
							}
							target = newTarget;
						}
					}
				}

				items.Add(item);
			}

			return items;
		}

		private object GetConstantObject(Assembly assembly, string sheetTypeName, Type sheetType)
		{
			var constantContents = _contents.OfType<ConstantContents>().First();
			var obj = Activator.CreateInstance(sheetType) ?? throw new Exception("Type does not exist.");
			constantContents.SetData(obj, _setting);
			return obj;
		}

		private void SetSerializeDesign( Type sheetType, bool value )
		{
			var propertyInfo = sheetType.GetProperty("_serializeDesign", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic) ?? throw new Exception("Property does not exist.");
			propertyInfo.SetValue(null, value);
		}

		public string Serialize(Assembly assembly)
		{
			var sheetTypeName =
				_sheetInfo.Name.ChangeNotation(_setting.InputNotation, _setting.ScriptClassNameNotation);
			var sheetType = assembly.GetType($"{_setting.NamespaceName ?? ""}.{sheetTypeName}")!;

			var hasConstant = HasContents<ConstantContents>();
			var hasDesign = HasContents<DesignContents>();

			var constantObject = hasConstant ? GetConstantObject(assembly, sheetTypeName, sheetType) : null;
			var designObject = hasDesign ? GetDesignObject(assembly, sheetTypeName, sheetType) : null;
			if (hasConstant && hasDesign)
			{
				SetSerializeDesign(sheetType, false);
				var constantJson = JsonConvert.SerializeObject(constantObject, Formatting.Indented);
				SetSerializeDesign(sheetType, true);
				var designJson = JsonConvert.SerializeObject(designObject, Formatting.Indented);
				return $"{{\n\"item1\" : \n{constantJson},\n\"item2\" : \n{designJson}\n}}";
			}

			if (hasConstant) {
				SetSerializeDesign(sheetType, false);
				return JsonConvert.SerializeObject(constantObject, Formatting.Indented);
			}

			if (hasDesign)
			{
				SetSerializeDesign(sheetType, true);
				return JsonConvert.SerializeObject(designObject, Formatting.Indented);
			}
			return string.Empty;
		}

		public override string ToString()
		{
			return string.Join('\n', _contents);
		}
	}
}