using System.Collections.Generic;
using System.Linq;

namespace SheetDataTool
{
	public class OverlapSheetNameException : SheetDataToolException
	{
		public struct OverlapInfo
		{
			public string SheetName;
			public string SheetPath;
		}

		public IEnumerable<OverlapInfo> OverlapInfos { get; }

		public OverlapSheetNameException(IEnumerable<OverlapInfo> overlapInfos)
		{
			OverlapInfos = overlapInfos.ToList();
		}

		public override string ToString()
		{
			return $"There are multiple sheets with the same name.({string.Join(", ", OverlapInfos.Select(x => $"(Path : {x.SheetPath}, Name : {x.SheetName})"))})";
		}
	}
}
