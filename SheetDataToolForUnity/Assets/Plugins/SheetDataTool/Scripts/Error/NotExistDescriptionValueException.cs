namespace SheetDataTool
{
	public class NotExistDescriptionValueException : SheetDataToolException, ISheetReference
	{
		public int Row { get; }
		public int Column => 0;

		public NotExistDescriptionValueException(int row)
		{
			Row = row;
		}

		public override string ToString()
		{
			return $"Description value does not exist.(Cell reference : {ReferenceUtil.GetReference(Row, 0)})";
		}
	}
}
