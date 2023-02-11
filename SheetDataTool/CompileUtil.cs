using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace SheetDataTool
{
	internal static class CompileUtil
	{
		public static Assembly Compile( string assemblyName, params string[] scripts )
		{
			var syntaxTrees = scripts.Select(x => CSharpSyntaxTree.ParseText(x));
			var refPaths = new[] 
			{
				typeof(object).GetTypeInfo().Assembly.Location,
				typeof(Console).GetTypeInfo().Assembly.Location,
				typeof(Task).GetTypeInfo().Assembly.Location,
				typeof(Enumerable).GetTypeInfo().Assembly.Location,
				typeof(JsonConvert).GetTypeInfo().Assembly.Location,
				Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location) ?? string.Empty, "System.Runtime.dll")
			};
			var references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
			var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);

			using var ms = new MemoryStream();
			var result = compilation.Emit(ms);

			if (!result.Success) 
			{
				throw new Exception($"Compilation failed.{string.Join('\n', result.Diagnostics)}");
			}
			ms.Seek(0, SeekOrigin.Begin);
			var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
			return assembly;
		}
	}
}