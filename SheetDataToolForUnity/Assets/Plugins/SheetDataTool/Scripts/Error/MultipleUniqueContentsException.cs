namespace SheetDataTool
{
	public class MultipleUniqueContentsException : SheetDataToolException
	{
		public string ContentsName { get; }
		public int Row { get; }

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
