using System.Reflection;

namespace SheetDataTool
{
	internal static class TypeUtil
	{
		public static HashSet<string> UnityTypeNames = new()
		{
			"Vector2",
			"Vetctor3",
			"Vector2Int",
			"Vector3Int",
			"Color",
		};

		public static object? ChangeType( string text, Type type ) 
		{
			if (type == typeof(string)) 
			{
				return text;
			}

			if (type.IsEnum)
			{
				return Enum.Parse(type, text);
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				if (string.IsNullOrWhiteSpace(text) || text == "null") return null;
				return Convert.ChangeType(text, type.GenericTypeArguments[0]);
			}

			return Convert.ChangeType(text, type);
		}

		public static IEnumerable<Type> GetChildClasses<T>()
		{
			return Assembly.GetAssembly(typeof(T))?
				.GetTypes()
				.Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(T))) 
			       ?? Enumerable.Empty<Type>();
		}

		private static readonly Dictionary<string, Type> BasicTypes = new()
		{
			{"int", typeof(int)},
			{"uint", typeof(uint)},
			{"short", typeof(short)},
			{"ushort", typeof(ushort)},
			{"long", typeof(long)},
			{"ulong", typeof(ulong)},
			{"byte", typeof(byte)},
			{"sbyte", typeof(sbyte)},
			{"float", typeof(float)},
			{"double", typeof(double)},
			{"bool", typeof(bool)},
			{"string", typeof(string)},
			{"int?", typeof(int?)},
			{"uint?", typeof(uint?)},
			{"short?", typeof(short?)},
			{"ushort?", typeof(ushort?)},
			{"long?", typeof(long?)},
			{"ulong?", typeof(ulong?)},
			{"byte?", typeof(byte?)},
			{"sbyte?", typeof(sbyte?)},
			{"float?", typeof(float?)},
			{"double?", typeof(double?)},
			{"bool?", typeof(bool?)},
		};

		public static bool IsBasicType(string typeName) => BasicTypes.ContainsKey(typeName);

		public static Type? GetType(string typeName, Assembly assembly)
		{
			if (BasicTypes.TryGetValue(typeName, out var basicType))
			{
				return basicType;
			}

			return assembly.DefinedTypes.FirstOrDefault(x => x.Name == typeName);
		}
	}
}
