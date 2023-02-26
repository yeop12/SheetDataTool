using System;

namespace SheetDataTool
{
	[Serializable]
	public record GoogleSheetAccessInfo
	{
		public string OAuthFilePath { get; set; }
		public string SheetID { get; set; }
	}
}
