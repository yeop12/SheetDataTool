namespace SheetDataTool
{
	public class FailedCompileException : SheetDataToolException
	{
		public FailedCompileException(string message) : base(message)
		{
		}

		public override string ToString()
		{
			return $"Compile failed.(Reason : {Message})";
		}
	}
}
