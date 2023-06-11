using System.Collections.Generic;

namespace SheetDataTool
{
	public interface SheetUtil
	{
		public IEnumerable<string> GetSheetNames();
		public SheetInfo GetSheetInfo(string sheetName);
		public void RefreshSheetList();
	}
}
