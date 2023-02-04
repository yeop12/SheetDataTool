namespace SheetDataTool
{
	public class InvalidSheetRuleException : Exception
	{
		public int? Row { get; }
		public int? Column { get; }

		public InvalidSheetRuleException( string message, int? row = null, int? column = null ) : base($"{message}(Row : {row}, Column : {column})")
		{
			Row = row;
			Column = column;
		}
	}
}