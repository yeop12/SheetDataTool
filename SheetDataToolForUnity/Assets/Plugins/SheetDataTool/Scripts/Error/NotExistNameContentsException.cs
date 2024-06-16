namespace SheetDataTool
{
	public class NotExistNameContentsException : SheetDataToolException
	{
		public override string ToString()
		{
			return $"Name contents does not exist.";
		}
	}
}
