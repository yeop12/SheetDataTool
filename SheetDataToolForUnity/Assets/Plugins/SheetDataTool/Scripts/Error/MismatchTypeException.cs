using System;

namespace SheetDataTool
{
	public class MismatchTypeException : SheetDataToolException, ISheetReference
	{
		public Type Type { get; }
		public string Value { get; }
		public int Row { get; }
		public int Column { get; }

		public MismatchTypeException(Type type, string value, int row, int column)
		{
			Type = type;
			Value = value;
			Row = row;
			Column = column;
		}

		public override string ToString()
		{
			return $"The value does not match the type.(Value : {Value}, Type : {Type.Name}, Cell reference : {ReferenceUtil.GetReference(Row, Column)})";
		}
	}
}
