namespace SheetDataTool
{
	public class NotExistNameContentsElementException : SheetDataToolException, ISheetReference
	{
		public int Row { get; }
		public int Column => 0;

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
