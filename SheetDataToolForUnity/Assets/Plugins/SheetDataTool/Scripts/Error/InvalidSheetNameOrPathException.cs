namespace SheetDataTool
{
	internal class InvalidSheetNameOrPathException : SheetDataToolException
	{
		public string SheetName { get; }
		public string? SheetPath { get; }

		public InvalidSheetNameOrPathException(string sheetName, string? sheetPath)
		{
			SheetName = sheetName;
			SheetPath = sheetPath;
		}

		public override string ToString()
		{
			return SheetPath is null ? $"'{SheetName}' sheet does not exist." : $"'{SheetName}' is an invalid sheet name.(Sheet path : {SheetPath})";
		}
	}
}
