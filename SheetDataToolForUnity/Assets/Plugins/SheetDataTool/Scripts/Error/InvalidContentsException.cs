namespace SheetDataTool
{
	public class InvalidContentsException : SheetDataToolException
	{
		public string ContentsName { get; }
		public int Row { get; }

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
