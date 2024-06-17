using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SheetDataTool
{
	public sealed record Setting
	{
		public Notation InputNotation { get; set; } = Notation.Pascal;
		public Notation ScriptRecordNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptRecordPropertyNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptEnumNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptEnumItemNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptClassNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptPublicVariableNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptPrivateVariableNameNotation { get; set; } = Notation.Camel;
		public string ScriptPrivateVariableNamePrefix { get; set; } = "_";
		public Notation ScriptInterfaceNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptFunctionNameNotation { get; set; } = Notation.Pascal;
		public string ScriptInterfaceNamePrefix { get; set; } = "I";
		public string IgnoreLineSymbol { get; set; } = ";";
		public string EnumDefaultType { get; set; } = "int";
		public string NamespaceName { get; set; } = "Sheet";
		public List<PlatformInfo> PlatformInfos { get; set; } = new();

		public bool IsIgnoreCell([NotNullWhen(false)]string? cell) => string.IsNullOrWhiteSpace(cell) || cell.StartsWith(IgnoreLineSymbol);
	}
}
