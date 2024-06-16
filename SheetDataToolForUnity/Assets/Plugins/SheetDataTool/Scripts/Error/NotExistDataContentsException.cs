namespace SheetDataTool
{
	public class NotExistDataContentsException : SheetDataToolException
	{
		public override string ToString()
		{
			return "'Data' contents does not exist.";
		}
	}
}
