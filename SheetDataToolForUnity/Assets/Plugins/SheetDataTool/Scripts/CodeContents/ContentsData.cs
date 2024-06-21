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

		public bool HasDataFile =>
			GetScriptType(_contents.OfType<ConstantContents>().Any(), _contents.OfType<DesignContents>().Any()) is not ScriptType.None;
		
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
				
				var contentsTypeName = CodeContents.GetContentsTypeName(firstCell)?.ChangeNotation(setting.InputNotation, Notation.Pascal);
				if (contentsTypeName is null or "Name" or "Data")
				{
					break;
				}

				if(contentsInfos.TryGetValue(contentsTypeName, out var contentsInfo))
				{
					if (contentsInfo.CanRegisterMultiple is false)
					{
						if (addedContentNames.Add(contentsTypeName) is false)
						{
							throw new MultipleUniqueContentsException(contentsTypeName, row);
						}
					}
				}
				else
				{
					throw new InvalidContentsException(contentsTypeName, row);
				}
				
				var endRow = sheetInfo.FindRow(row + 1, 0, x => string.IsNullOrWhiteSpace(x) || CodeContents.IsContentsCell(x));
				if (endRow == -1) endRow = sheetInfo.RowCount;
				var sheetInfoView = new SheetInfoView(sheetInfo, row, 0, endRow - 1, sheetInfo.ColumnCount - 1);
				var contents = (contentsInfo.ConstructorInfo.Invoke(BindingFlags.DoNotWrapExceptions, null, new object[] { sheetInfoView, setting }, null) as CodeContents)!;
				_contents.Add(contents);

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
			var sheetName = _setting.ToRecordName(_sheetInfo.Name);

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

		private void WriteUsingNamespaces(ScopedStringBuilder sb)
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

			foreach (var platformInfo in _setting.PlatformInfos)
			{
				sb.WriteLine($"#if {platformInfo.DefineName}");
				platformInfo.NamespaceNames.ForEach(x => sb.WriteLine($"using {x};"));
				sb.WriteLine($"#endif");
			}

			sb.WriteLine();
		}

		private static ScriptType GetScriptType(bool hasConstant, bool hasDesign)
		{
			return hasConstant switch
			{
				true when hasDesign => ScriptType.Full,
				true => ScriptType.Constant,
				_ => hasDesign ? ScriptType.Design : ScriptType.None
			};
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
						object? item = null;
						if (OutputType != typeof(string))
						{
							item = Activator.CreateInstance(OutputType);
						}
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
					: setting.ToPublicVariableName(value);
				_propertyInfo = parentType.GetProperty(propertyName) ?? throw new Exception("Property does not exist.");
				if (_propertyInfo.PropertyType.IsGenericType &&
				    _propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
				{
					propertyName = setting.ToPrivateVariableName(value);
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
			var nameRow =  _sheetInfo.FindRow(0, 0, x => CodeContents.IsContentsCell(x) && CodeContents.GetContentsTypeName(x) == "Name".ChangeNotation(Notation.Pascal, _setting.InputNotation));
			if (nameRow == -1)
			{
				throw new NotExistNameContentsException();
			}

			if (nameRow == _sheetInfo.RowCount - 1)
			{
				throw new NotExistNameContentsElementException(nameRow);
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
					catch
					{
						throw new InvalidNameContentsElementException(name, nameRow + 1, column);
					}
				}

				accessInfosByColumn.Add(column, properties);
			}

			var dataRow = _setting.UseDataContents
				? _sheetInfo.FindRow(nameRow + 2, 0,
					x => CodeContents.IsContentsCell(x) && CodeContents.GetContentsTypeName(x) ==
						"Data".ChangeNotation(Notation.Pascal, _setting.InputNotation))
				: nameRow + 1;
			if (dataRow == -1)
			{
				throw new NotExistDataContentsException();
			}

			var items = (Activator.CreateInstance(typeof(List<>).MakeGenericType(sheetType)) as IList)!;
			var keys = new HashSet<object>();

			for (var row = dataRow + 1; row < _sheetInfo.RowCount; ++row)
			{
				if (_setting.IsIgnoreCell(_sheetInfo[row, 0])) continue;
				var item = Activator.CreateInstance(sheetType) ?? throw new Exception("Type does not exist.");
				foreach (var (column, accessInfos) in accessInfosByColumn)
				{
					var target = item;
					var cell = _sheetInfo[row, column];
					if (string.IsNullOrWhiteSpace(cell))
					{
						var isListItem = accessInfos.Any(x =>
							x.OutputType.IsGenericType && x.OutputType.GetGenericTypeDefinition() == typeof(List<>));
						if (isListItem) continue;
						var lastType = accessInfos.Last().OutputType;
						if (lastType.IsGenericType && lastType.GetGenericTypeDefinition() == typeof(Nullable<>)) continue;
						throw new NotExistValueException(row, column);
					}

					for (var i = 0; i < accessInfos.Count; ++i)
					{
						var accessInfo = accessInfos[i];
						if (i == accessInfos.Count - 1)
						{
							try
							{
								var value = TypeUtil.ChangeType(cell, accessInfo.OutputType);
								accessInfo.SetValue(target, value);
							}
							catch
							{
								throw new MismatchTypeException(accessInfo.OutputType, cell, row, column);
							}
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

				var key = item.GetType().GetProperty("Key")!.GetValue(item);
				if (keys.Add(key) is false)
				{
					throw new DuplicationKeyException($"{key}", row, 0);
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
			var sheetTypeName = _setting.ToRecordName(_sheetInfo.Name);
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