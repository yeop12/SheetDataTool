namespace SheetDataTool
{
	public class InvalidNameContentsElementException : SheetDataToolException, ISheetReference
	{
		public string Name { get; }
		public int Row { get; }
		public int Column { get; }

		public InvalidNameContentsElementException(string name, int row, int column)
		{
			Name = name;
			Row = row;
			Column = column;
		}

		public override string ToString()
		{
			return $"'{Name}' is an invalid name contents element.(Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
