using System.Collections.Generic;

namespace SheetDataTool
{
	public sealed record PlatformInfo
	{
		public Platform Platform { get; set; }
		public string DefineName { get; set; } = string.Empty;
		public List<string> NamespaceNames { get; set; } = new();
		public string DefaultDirectory { get; set; } = "SheetData";
		public string ScriptPath { get; set; } = string.Empty;
		public string DataPath { get; set; } = string.Empty;
	}
}
