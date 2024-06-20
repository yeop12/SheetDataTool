namespace SheetDataTool
{
	public class NotExistEssentialContentsElement : SheetDataToolException, ISheetReference
	{
		public int Row { get; }
		public int Column { get; }

		public NotExistEssentialContentsElement(int row, int column)
		{
			Row = row;
			Column = column;
		}

		public override string ToString()
		{
			return $"Essential contents element does not exist.(Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
