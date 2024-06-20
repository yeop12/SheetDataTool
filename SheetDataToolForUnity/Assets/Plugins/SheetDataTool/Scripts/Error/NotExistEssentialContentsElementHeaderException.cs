namespace SheetDataTool
{
	public class NotExistEssentialContentsElementHeaderException : SheetDataToolException, ISheetReference
	{
		public string EssentialElementHeaderName { get; }
		public int Row { get; }
		public int Column => 0;

		public NotExistEssentialContentsElementHeaderException(string essentialElementHeaderName, int row)
		{
			EssentialElementHeaderName = essentialElementHeaderName;
			Row = row;
		}

		public override string ToString()
		{
			return $"The '{EssentialElementHeaderName}' contents element header must be included.(Cell reference : {ReferenceUtil.GetReference(Row, 0)})";
		}
	}
}
