namespace SheetDataTool
{
	public class NotExistNameContentsElementException : SheetDataToolException
	{
		public int Row { get; }

		public NotExistNameContentsElementException(int row)
		{
			Row = row;
		}

		public override string ToString()
		{
			return $"Name contents element does not exist.(Cell reference : {ReferenceUtil.GetReference(Row, 0)})";
		}
	}
}
