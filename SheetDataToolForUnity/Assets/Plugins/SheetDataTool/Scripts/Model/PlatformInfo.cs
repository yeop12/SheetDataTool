using System.Collections.Generic;
using System.IO;

namespace SheetDataTool
{
	public sealed record PlatformInfo
	{
		public Platform Platform { get; set; } = Platform.Unity;
		public string DefineName { get; set; } = "UNITY_2022_1_OR_NEWER";
		public List<string> NamespaceNames { get; set; } = new();
		public string DefaultDirectory { get; set; } = "SheetData";
		public string ScriptPath { get; set; } = string.Empty;
		public string DataPath { get; set; } = string.Empty;

		public bool IsValid()
		{
			if (string.IsNullOrWhiteSpace(DefaultDirectory))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(ScriptPath) || Directory.Exists(ScriptPath) is false)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(DataPath) || Directory.Exists(DataPath)is false)
			{
				return false;
			}

			return true;
		}
	}
}
