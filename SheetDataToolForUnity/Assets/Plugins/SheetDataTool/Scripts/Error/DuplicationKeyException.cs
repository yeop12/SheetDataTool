namespace SheetDataTool
{
	public class DuplicationKeyException : SheetDataToolException
	{
		public int Row { get; }
		public int Column { get; }
		public string Key { get; }

		public DuplicationKeyException(string key, int row, int column) : base()
		{
			Row = row;
			Column = column;
			Key = key;
		}

		public override string ToString()
		{
			return $"Duplication key exist.(Key : {Key}, Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
