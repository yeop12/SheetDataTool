using System.Diagnostics.CodeAnalysis;

namespace SheetDataTool
{
	public sealed record Setting
	{
		public Notation InputNotation { get; init; } = Notation.Pascal;
		public Notation ScriptRecordNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptRecordPropertyNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptEnumNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptEnumItemNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptClassNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptPublicVariableNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptPrivateVariableNameNotation { get; init; } = Notation.Camel;
		public string ScriptPrivateVariableNamePrefix { get; init; } = "_";
		public Notation ScriptInterfaceNameNotation { get; init; } = Notation.Pascal;
		public Notation ScriptFunctionNameNotation { get; init; } = Notation.Pascal;
		public string ScriptInterfaceNamePrefix { get; init; } = "I";
		public char IgnoreLineSymbol { get; init; } = ';';
		public string EnumDefaultType { get; init; } = "int";
		public string NamespaceName { get; init; } = "Sheet";
		public string DefaultDirectory { get; init; } = "SheetData";
		public string UnityPlatformDefine { get; init; } = "UNITY_2019_4_OR_NEWER";

		public bool IsIgnoreCell([NotNullWhen(false)]string? cell) => string.IsNullOrWhiteSpace(cell) || cell.StartsWith(IgnoreLineSymbol);
	}
}
