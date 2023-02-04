using System.Reflection;

namespace SheetDataTool
{
	internal class TypeUtil
	{
		public static object ChangeType( string text, Type type ) 
		{
			if (type == typeof(string)) 
			{
				return text;
			}
			else if (type.IsEnum)
			{
				return Enum.Parse(type, text);
			}
			else
			{
				return Convert.ChangeType(text, type);
			}
		}

		public static IEnumerable<Type> GetChildClasses<T>()
		{
			return Assembly.GetAssembly(typeof(T))?
				.GetTypes()
				.Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(T))) 
			       ?? Enumerable.Empty<Type>();
		}
	}
}
