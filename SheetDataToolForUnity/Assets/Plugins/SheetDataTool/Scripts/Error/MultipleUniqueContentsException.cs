namespace SheetDataTool
{
	public class MultipleUniqueContentsException : SheetDataToolException, ISheetReference
	{
		public string ContentsName { get; }
		public int Row { get; }
		public int Column => 0;

		public MultipleUniqueContentsException(string contentsName, int row)
		{
			ContentsName = contentsName;
			Row = row;
		}

		public override string ToString()
		{
			return $"Contents named '{ContentsName}' already exist.(Cell reference : {ReferenceUtil.GetReference(Row, 0)})";
		}
	}
}
