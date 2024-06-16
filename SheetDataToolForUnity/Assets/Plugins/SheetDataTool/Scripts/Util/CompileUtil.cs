using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace SheetDataTool
{
	public static class CompileUtil
	{
		public static Assembly Compile( string assemblyName, params string[] scripts )
		{
			var syntaxTrees = scripts.Select(x => CSharpSyntaxTree.ParseText(x));
			var runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
				.First(assembly => assembly.GetName().Name == "System.Runtime");
			var dotNetStandardAssembly = AppDomain.CurrentDomain.GetAssemblies()
				.First(assembly => assembly.GetName().Name == "netstandard");
			
			var refPaths = new[] 
			{
				typeof(object).GetTypeInfo().Assembly.Location,
				typeof(Console).GetTypeInfo().Assembly.Location,
				typeof(Task).GetTypeInfo().Assembly.Location,
				typeof(Enumerable).GetTypeInfo().Assembly.Location,
				typeof(JsonConvert).GetTypeInfo().Assembly.Location,
				runtimeAssembly.Location,
				dotNetStandardAssembly.Location,
			};
			var references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
			var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);

			using var ms = new MemoryStream();
			var result = compilation.Emit(ms);

			if (!result.Success)
			{
				throw new FailedCompileException(string.Join('\n', result.Diagnostics));
			}
			ms.Seek(0, SeekOrigin.Begin);
			var assembly = Assembly.Load(ms.ToArray());
			return assembly;
		}
	}
}