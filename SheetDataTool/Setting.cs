namespace SheetDataTool
{
	public sealed record Setting
	{
		public Notation InputNotation { get; init; } = Notation.Pascal;
		public Notation OutputNotation { get; init; } = Notation.Pascal;
		public char IgnoreLineSymbol { get; init; } = ';';
		public string EnumDefaultType { get; init; } = "int";

		public bool IsIgnoreCell(string? cell) => string.IsNullOrWhiteSpace(cell) || cell.StartsWith(IgnoreLineSymbol);
	}
}
