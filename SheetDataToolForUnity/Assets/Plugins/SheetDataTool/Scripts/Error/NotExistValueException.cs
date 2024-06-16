namespace SheetDataTool
{
	public class NotExistValueException : SheetDataToolException
	{
		public int Row { get; }
		public int Column { get; }

		public NotExistValueException(int row, int column)
		{
			Row = row;
			Column = column;
		}

		public override string ToString()
		{
			return $"Value does not exist. It cannot be blank except for list or nullable types.(Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
