using System;
using System.Linq;

namespace SheetDataTool
{
	public static class ScriptUtil
	{
		private static string ToFunctionName(this string name, Setting setting) =>
			name.ChangeNotation(Notation.Pascal, setting.ScriptFunctionNameNotation);

		public static string GetBaseClassName(Setting setting) => "BaseSheetDataHelper".ChangeNotation(Notation.Pascal, setting.ScriptClassNameNotation);

		public static string GetBaseClassScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();
			sb.WriteLine("using System.Linq;");
			sb.WriteLine("using System.Reflection;");
			sb.WriteLine("using System.Threading.Tasks;");
			foreach (var platformInfo in setting.PlatformInfos)
			{
				sb.WriteLine($"#if {platformInfo.DefineName}");
				platformInfo.NamespaceNames.ForEach(x => sb.WriteLine($"using {x};"));
				sb.WriteLine("#endif");
			}
			sb.WriteLine();

			var namespaceScope = string.IsNullOrWhiteSpace(setting.NamespaceName)
				? null : sb.StartScope($"namespace {setting.NamespaceName}");

			using (sb.StartScope($"public abstract record {GetBaseClassName(setting)}"))
			{
				foreach (var (platformInfo, index) in setting.PlatformInfos.Select((x, i) => (x, i)))
				{
					sb.WriteLine($"{(index == 0 ? "#if" : "#elif")} {platformInfo.DefineName}");
					sb.WriteLine($"protected static readonly string DefaultDirectory = \"{platformInfo.DefaultDirectory}\";");
				}
				sb.WriteLine("#else");
				sb.WriteLine($"protected static readonly string DefaultDirectory = string.Empty;");
				sb.WriteLine("#endif");
				sb.WriteLine();
				
				using (sb.StartScope($"protected static string {"ReadData".ToFunctionName(setting)}<T>()"))
				{
					foreach (var (settingPlatformInfo, index) in setting.PlatformInfos.Select((x, i) => (x, i)))
					{
						sb.WriteLine($"{(index == 0 ? "#if" : "#elif")} {settingPlatformInfo.DefineName}");

						switch (settingPlatformInfo.Platform)
						{
							case Platform.Unity:
								sb.WriteLine("var textAsset = Addressables.LoadAssetAsync<TextAsset>($\"{DefaultDirectory}/{typeof(T).Name}\").WaitForCompletion();");
								sb.WriteLine("var json = textAsset.text;");
								break;

							case Platform.CSharp:
								sb.WriteLine("var json = System.IO.File.ReadAllText($\"{DefaultDirectory}/{typeof(T).Name}.json\");");
								break;

							default:
								throw new NotImplementedException($"{settingPlatformInfo.Platform}");
						}
					}
					sb.WriteLine("#else");
					sb.WriteLine("var json = string.Empty;");
					sb.WriteLine("#endif");
					sb.WriteLine();

					sb.WriteLine("return json;");
				}
				sb.WriteLine();
				
				using (sb.StartScope($"protected static async Task<string> {"ReadDataAsync".ToFunctionName(setting)}<T>()"))
				{
					foreach (var (settingPlatformInfo, index) in setting.PlatformInfos.Select((x, i) => (x, i)))
					{
						sb.WriteLine($"{(index == 0 ? "#if" : "#elif")} {settingPlatformInfo.DefineName}");

						switch (settingPlatformInfo.Platform)
						{
							case Platform.Unity:
								sb.WriteLine("var textAsset = await Addressables.LoadAssetAsync<TextAsset>($\"{DefaultDirectory}/{typeof(T).Name}\").Task;");
								sb.WriteLine("var json = textAsset.text;");
								break;

							case Platform.CSharp:
								sb.WriteLine("var json = await System.IO.File.ReadAllTextAsync($\"{DefaultDirectory}/{typeof(T).Name}.json\");");
								break;

							default:
								throw new NotImplementedException($"{settingPlatformInfo.Platform}");
						}
					}
					sb.WriteLine("#else");
					sb.WriteLine("var json = string.Empty;");
					sb.WriteLine("#endif");
					sb.WriteLine();

					sb.WriteLine("return json;");
				}
				sb.WriteLine();

				using (sb.StartScope($"public static async Task {"LoadAllDataAsync".ToFunctionName(setting)}()"))
				{
					sb.WriteLine($"var sheetTypes = typeof({GetBaseClassName(setting)}).Assembly.GetTypes().Where(x =>x.IsAbstract is false && x.IsSubclassOf(typeof({GetBaseClassName(setting)})));");
					sb.WriteLine($"var methodInfos = sheetTypes.Select(x => x.GetMethod(\"{"LoadDataAsync".ToFunctionName(setting)}\", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public));");
					sb.WriteLine("var loadFunctions = methodInfos.Select(x => x?.Invoke(null, new object[] { })).Cast<Task>();");
					sb.WriteLine("await Task.WhenAll(loadFunctions);");
				}
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		public static string GetDesignInterfaceName(Setting setting) => $"{setting.ScriptInterfaceNamePrefix}{"DesignSheetData".ChangeNotation(Notation.Pascal, setting.ScriptInterfaceNameNotation)}";

		public static string GetDesignInterfaceScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();

			var namespaceScope = string.IsNullOrWhiteSpace(setting.NamespaceName)
				? null : sb.StartScope($"namespace {setting.NamespaceName}");

			using (sb.StartScope($"public interface {GetDesignInterfaceName(setting)}<out T>"))
			{
				sb.WriteLine($"public T {"Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation)} {{ get; }}");
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		public static string GetDesignClassName(Setting setting) => "DesignSheetDataHelper".ChangeNotation(Notation.Pascal, setting.ScriptClassNameNotation);

		public static string GetDesignClassScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();
			sb.WriteLine("using System;");
			sb.WriteLine("using System.Collections.Generic;");
			sb.WriteLine("using System.Linq;");
			sb.WriteLine("using System.Threading.Tasks;");
			sb.WriteLine("using Newtonsoft.Json;");
			sb.WriteLine();

			var namespaceScope = string.IsNullOrWhiteSpace(setting.NamespaceName)
				? null : sb.StartScope($"namespace {setting.NamespaceName}");

			using (sb.StartScope($"public abstract record {GetDesignClassName(setting)}<TKey, TValue> : {GetBaseClassName(setting)} where TValue : {GetDesignInterfaceName(setting)}<TKey>"))
			{
				var dataPrivateName = $"{setting.ScriptPrivateVariableNamePrefix}{"data".ChangeNotation(Notation.Camel, setting.ScriptPrivateVariableNameNotation)}";
				sb.WriteLine($"private static Lazy<Dictionary<TKey, TValue>> {dataPrivateName} = new({"LoadData".ToFunctionName(setting)});");
				sb.WriteLine();

				sb.WriteLine($"public static IEnumerable<TValue> {"Data".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation)} => {dataPrivateName}.Value.Values;");
				sb.WriteLine();

				using (sb.StartScope($"private static Dictionary<TKey, TValue> {"LoadData".ToFunctionName(setting)}()"))
				{
					var keyName = "Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation);
					sb.WriteLine($"var json = {"ReadData".ToFunctionName(setting)}<TValue>();");
					sb.WriteLine();
					using (sb.StartScope("if (JsonConvert.DeserializeObject<List<TValue>>(json) is not { } data)")) 
					{
						sb.WriteLine("throw new Exception($\"{typeof(TValue).Name} does not loaded.\");");
					}
					sb.WriteLine($"return data.ToDictionary(x => x.{keyName}, x => x);");
				}
				sb.WriteLine();
				
				using (sb.StartScope($"public static async Task {"LoadDataAsync".ToFunctionName(setting)}()"))
				{
					var keyName = "Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation);
					sb.WriteLine($"var json = await {"ReadDataAsync".ToFunctionName(setting)}<TValue>();");
					sb.WriteLine();

					using (sb.StartScope("if (JsonConvert.DeserializeObject<List<TValue>>(json) is not { } data)")) 
					{
						sb.WriteLine("throw new Exception($\"{typeof(TValue).Name} does not loaded.\");");
					}
					sb.WriteLine();

					sb.WriteLine($"var dataDictionary = data.ToDictionary(x => x.{keyName}, x => x);");
					sb.WriteLine($"{dataPrivateName} = new Lazy<Dictionary<TKey, TValue>>(dataDictionary);");
				}
				sb.WriteLine();

				var findFunctionName = "Find".ChangeNotation(Notation.Pascal, setting.ScriptFunctionNameNotation);
				using (sb.StartScope($"public static TValue {findFunctionName}(TKey key, bool throwNotFoundException = false)"))
				{
					using (sb.StartScope($"if ({dataPrivateName}.Value.TryGetValue(key, out var result) is false && throwNotFoundException)"))
					{
						sb.WriteLine("throw new ExcelDataNotFoundException<TValue>(key);");
					}
					sb.WriteLine("return result;");
				}
				sb.WriteLine();

				using (sb.StartScope($"public static TValue {findFunctionName}( Predicate<TValue> match )"))
				{
					sb.WriteLine($"return {dataPrivateName}.Value.Values.FirstOrDefault(match.Invoke);");
				}
				sb.WriteLine();

				var findAllFunctionName = "FindAll".ChangeNotation(Notation.Pascal, setting.ScriptFunctionNameNotation);
				using (sb.StartScope($"public static IEnumerable<TValue> {findAllFunctionName}( Predicate<TValue> match ) "))
				{
					sb.WriteLine($"return {dataPrivateName}.Value.Values.Where(match.Invoke);");
				}
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		public static string GetConstantClassName(Setting setting) => "ConstantSheetDataHelper".ChangeNotation(Notation.Pascal, setting.ScriptClassNameNotation);

		public static string GetConstantClassScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();
			sb.WriteLine("using System;");
			sb.WriteLine("using System.Threading.Tasks;");
			sb.WriteLine("using Newtonsoft.Json;");
			sb.WriteLine();

			var namespaceScope = string.IsNullOrWhiteSpace(setting.NamespaceName)
				? null : sb.StartScope($"namespace {setting.NamespaceName}");

			using (sb.StartScope($"public abstract record {GetConstantClassName(setting)}<T> : {GetBaseClassName(setting)}"))
			{
				sb.WriteLine("protected static bool IsLoaded;");
				sb.WriteLine();
				
				using (sb.StartScope($"protected static void {"LoadData".ToFunctionName(setting)}()"))
				{
					sb.WriteLine("IsLoaded = true;");
					sb.WriteLine($"var json = {"ReadData".ToFunctionName(setting)}<T>();");
					sb.WriteLine();

					sb.WriteLine("if (JsonConvert.DeserializeObject<T>(json) is { }) return;");
					sb.WriteLine("throw new Exception($\"{ typeof(T).Name} does not loaded.\");");
				}
				sb.WriteLine();

				using (sb.StartScope($"public static async Task {"LoadDataAsync".ToFunctionName(setting)}()"))
				{
					sb.WriteLine("IsLoaded = true;");
					sb.WriteLine($"var json = await {"ReadDataAsync".ToFunctionName(setting)}<T>();");
					sb.WriteLine();

					sb.WriteLine("if (JsonConvert.DeserializeObject<T>(json) is { }) return;");
					sb.WriteLine("throw new Exception($\"{ typeof(T).Name} does not loaded.\");");
				}
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		public static string GetFullClassName(Setting setting) => "FullSheetDataHelper".ChangeNotation(Notation.Pascal, setting.ScriptClassNameNotation);

		public static string GetFullClassScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();
			sb.WriteLine("using System;");
			sb.WriteLine("using System.Collections.Generic;");
			sb.WriteLine("using System.Linq;");
			sb.WriteLine("using System.Threading.Tasks;");
			sb.WriteLine("using Newtonsoft.Json;");
			sb.WriteLine();

			var namespaceScope = string.IsNullOrWhiteSpace(setting.NamespaceName)
				? null : sb.StartScope($"namespace {setting.NamespaceName}");

			using (sb.StartScope($"public abstract record {GetFullClassName(setting)}<TKey, TValue> : {GetBaseClassName(setting)} where TValue : {GetDesignInterfaceName(setting)}<TKey>"))
			{
				sb.WriteLine("protected static bool IsLoaded;");
				var dataPrivateName = $"{setting.ScriptPrivateVariableNamePrefix}{"data".ChangeNotation(Notation.Camel, setting.ScriptPrivateVariableNameNotation)}";
				sb.WriteLine($"private static Dictionary<TKey, TValue> {dataPrivateName};");
				var dataPublicName = "Data".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation);
				var loadDataFunctionName = "LoadData".ToFunctionName(setting);
				using (sb.StartScope($"public static IEnumerable<TValue> {dataPublicName}"))
				{
					using (sb.StartScope("get"))
					{
						sb.WriteLine($"if (IsLoaded is false) {loadDataFunctionName}();");
						sb.WriteLine($"return {dataPrivateName}.Values;");
					}
				}
				sb.WriteLine();

				using (sb.StartScope($"protected static void {loadDataFunctionName}()"))
				{
					sb.WriteLine("IsLoaded = true;");
					sb.WriteLine($"var json = {"ReadData".ToFunctionName(setting)}<TValue>();");
					sb.WriteLine();

					sb.WriteLine("var (_ ,data) = JsonConvert.DeserializeObject<(TValue, List<TValue>)>(json);");
					using (sb.StartScope("if(data is null)"))
					{
						sb.WriteLine("throw new Exception($\"{typeof(TValue).Name} does not loaded.\");");
					}
					var keyName = "Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation);
					sb.WriteLine($"{dataPrivateName} = data.ToDictionary(x => x.{keyName}, x => x);");

				}
				sb.WriteLine();

				using (sb.StartScope($"public static async Task {"LoadDataAsync".ToFunctionName(setting)}()"))
				{
					sb.WriteLine("IsLoaded = true;");
					sb.WriteLine($"var json = await {"ReadDataAsync".ToFunctionName(setting)}<TValue>();");
					sb.WriteLine();

					sb.WriteLine("var (_ ,data) = JsonConvert.DeserializeObject<(TValue, List<TValue>)>(json);");
					using (sb.StartScope("if(data is null)"))
					{
						sb.WriteLine("throw new Exception($\"{typeof(TValue).Name} does not loaded.\");");
					}
					var keyName = "Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation);
					sb.WriteLine($"{dataPrivateName} = data.ToDictionary(x => x.{keyName}, x => x);");

				}
				sb.WriteLine();

				var findFunctionName = "Find".ChangeNotation(Notation.Pascal, setting.ScriptFunctionNameNotation);
				using (sb.StartScope($"public static TValue {findFunctionName}(TKey key, bool throwNotFoundException = false)"))
				{
					sb.WriteLine($"if (IsLoaded is false) {loadDataFunctionName}();");
					using (sb.StartScope($"if ({dataPrivateName}.TryGetValue(key, out var result) is false && throwNotFoundException)"))
					{
						sb.WriteLine("throw new ExcelDataNotFoundException<TValue>(key);");
					}
					sb.WriteLine("return result;");
				}
				sb.WriteLine();

				using (sb.StartScope($"public static TValue {findFunctionName}( Predicate<TValue> match )"))
				{
					sb.WriteLine($"if (IsLoaded is false) {loadDataFunctionName}();");
					sb.WriteLine($"return {dataPrivateName}.Values.FirstOrDefault(match.Invoke);");
				}
				sb.WriteLine();

				var findAllFunctionName = "FindAll".ChangeNotation(Notation.Pascal, setting.ScriptFunctionNameNotation);
				using (sb.StartScope($"public static IEnumerable<TValue> {findAllFunctionName}( Predicate<TValue> match ) "))
				{
					sb.WriteLine($"if (IsLoaded is false) {loadDataFunctionName}();");
					sb.WriteLine($"return {dataPrivateName}.Values.Where(match.Invoke);");
				}
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		public static string GetExcelDataNotFoundExceptionName(Setting setting) => "ExcelDataNotFoundException".ChangeNotation(Notation.Pascal, setting.ScriptClassNameNotation);

		public static string GetExcelDataNotFoundExceptionScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();

			sb.WriteLine("using System;");
			sb.WriteLine();

			var namespaceScope = string.IsNullOrWhiteSpace(setting.NamespaceName)
				? null : sb.StartScope($"namespace {setting.NamespaceName}");

			var className = GetExcelDataNotFoundExceptionName(setting);
			using (sb.StartScope($"public class {className}<T> : Exception"))
			{
				var keyName = "Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation);
				sb.WriteLine($"public Type {"DataType".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation)} => typeof(T);");
				sb.WriteLine($"public Object {keyName} {{ get; private set; }}");
				using (sb.StartScope($"public {className}(object key) : base($\"{{typeof(T).Name}} does not exist.({keyName} : {{key}})\")"))
				{
					sb.WriteLine($"{keyName} = key;");
				}
			}

			namespaceScope?.Dispose();
			return sb.ToString();
		}

		public static string GetExternalInitName() => "IsExternalInit";

		public static string GetExternalInitScript(Setting setting)
		{
			var sb = new ScopedStringBuilder();
			using (sb.StartScope("namespace System.Runtime.CompilerServices"))
			{
				using (sb.StartScope($"public sealed class {GetExternalInitName()}")) ;
			}
			return sb.ToString();
		}

		public static string GetUnityTypeScript()
		{
			var sb = new ScopedStringBuilder();
			using (sb.StartScope("public record Vector2"))
			{
				sb.WriteLine("public float x { get; init; }");
				sb.WriteLine("public float y { get; init; }");
			}
			sb.WriteLine();
			
			using (sb.StartScope("public record Vector3"))
			{
				sb.WriteLine("public float x { get; init; }");
				sb.WriteLine("public float y { get; init; }");
				sb.WriteLine("public float z { get; init; }");
			}
			sb.WriteLine();
			
			using (sb.StartScope("public record Vector2Int"))
			{
				sb.WriteLine("public int x { get; init; }");
				sb.WriteLine("public int y { get; init; }");
			}
			sb.WriteLine();
			
			using (sb.StartScope("public record Vector3Int"))
			{
				sb.WriteLine("public int x { get; init; }");
				sb.WriteLine("public int y { get; init; }");
				sb.WriteLine("public int z { get; init; }");
			}
			sb.WriteLine();
			
			using (sb.StartScope("public record Color"))
			{
				sb.WriteLine("public float r { get; init; }");
				sb.WriteLine("public float g { get; init; }");
				sb.WriteLine("public float b { get; init; }");
				sb.WriteLine("public float a { get; init; }");
			}

			return sb.ToString();
		}
	}
}
