using System;

namespace SheetDataTool
{
	public class InvalidContentsElementHeaderException : SheetDataToolException
	{
		public string ElementHeaderName { get; }
		public int Row { get; }
		public int Column { get; }
		public Type ContentsElementType { get; }

		public InvalidContentsElementHeaderException(string elementHeaderName, int row, int column, Type contentsElementType)
		{
			ElementHeaderName = elementHeaderName;
			Row = row;
			Column = column;
			ContentsElementType = contentsElementType;
		}

		public override string ToString()
		{
			return $"'{ElementHeaderName} is an invalid contents element header.(Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
