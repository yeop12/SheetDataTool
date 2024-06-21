using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SheetDataTool
{
	public sealed record Setting
	{
		public Notation InputNotation { get; set; } = Notation.Pascal;
		public string IgnoreLineSymbol { get; set; } = ";";
		public string EnumDefaultType { get; set; } = "int";

		public Notation ScriptEnumNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptEnumItemNameNotation { get; set; } = Notation.Pascal;

		public Notation ScriptRecordNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptPublicVariableNameNotation { get; set; } = Notation.Pascal;
		public Notation ScriptPrivateVariableNameNotation { get; set; } = Notation.Camel;
		public string ScriptPrivateVariableNamePrefix { get; set; } = "_";

		public Notation ScriptInterfaceNameNotation { get; set; } = Notation.Pascal;
		public string ScriptInterfaceNamePrefix { get; set; } = "I";

		public Notation ScriptFunctionNameNotation { get; set; } = Notation.Pascal;

		public string NamespaceName { get; set; } = "Sheet";
		public bool UseDataContents { get; set; } = true;
		public List<PlatformInfo> PlatformInfos { get; set; } = new();

		public bool IsIgnoreCell([NotNullWhen(false)]string? cell) => string.IsNullOrWhiteSpace(cell) || cell.StartsWith(IgnoreLineSymbol);
		public string ToRecordName(string value) => value.ChangeNotation(InputNotation, ScriptRecordNameNotation);
		public string ToPublicVariableName(string value) => value.ChangeNotation(InputNotation, ScriptPublicVariableNameNotation);
		public string ToPrivateVariableName(string value) => $"{ScriptPrivateVariableNamePrefix}{value.ChangeNotation(InputNotation, ScriptPrivateVariableNameNotation)}";
		public string PascalToFunctionName(string value) => value.ChangeNotation(Notation.Pascal, ScriptFunctionNameNotation);
	}
}
