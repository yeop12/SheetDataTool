namespace SheetDataTool
{
	public class InvalidDirectoryException : SheetDataToolException
	{
		public string Path { get; }

		public InvalidDirectoryException(string path)
		{
			Path = path;
		}

		public override string ToString()
		{
			return $"'{Path}' is an invalid directory.";
		}
	}
}
