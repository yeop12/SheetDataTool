namespace SheetDataTool
{
	public class InvalidContentsException : SheetDataToolException, ISheetReference
	{
		public string ContentsName { get; }
		public int Row { get; }
		public int Column => 0;

		public InvalidContentsException(string contentsName, int row)
		{
			ContentsName = contentsName;
			Row = row;
		}

		public override string ToString()
		{
			return $"'{ContentsName}' is an invalid contents.(Cell reference : {ReferenceUtil.GetReference(Row, 0)})";
		}
	}
}
