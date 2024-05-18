namespace SheetDataTool
{
	public class NotExistEssentialContentsElementHeaderException : SheetDataToolException
	{
		public string EssentialElementHeaderName { get; }
		public int Row { get; }

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
