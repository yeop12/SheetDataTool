namespace SheetDataTool
{
	public class NotExistContentsNameException : SheetDataToolException, ISheetReference
	{
		public string ContentsTypeName { get; }
		public int Row { get; }
		public int Column { get; }

		public NotExistContentsNameException(string contentsTypeName, int row, int column)
		{
			ContentsTypeName = contentsTypeName;
			Row = row;
			Column = column;
		}

		public override string ToString()
		{
			return $"'{ContentsTypeName}' contents name does not exist.(Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
