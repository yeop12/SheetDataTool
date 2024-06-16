namespace SheetDataTool
{
	public class NotExistDescriptionValueException : SheetDataToolException
	{
		public int Row { get; }

		public NotExistDescriptionValueException(int row)
		{
			Row = row;
		}

		public override string ToString()
		{
			return $"Description value does not exist.(Cell reference : {ReferenceUtil.GetReference(Row + 1, 0)})";
		}
	}
}
