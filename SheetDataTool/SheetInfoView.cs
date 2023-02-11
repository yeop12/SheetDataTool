namespace SheetDataTool
{
	internal sealed class SheetInfoView
	{
		private readonly SheetInfo _sheetInfo;
		private readonly int _startRow;
		private readonly int _startColumn;

		public string? this[int row, int column] => _sheetInfo[row + _startRow, column + _startColumn];
		public int RowCount { get; }
		public int ColumnCount { get; }

		public SheetInfoView( SheetInfo sheetInfo, int startRow, int startColumn, int endRow, int endColumn )
		{
			if (startRow < 0 || startRow >= sheetInfo.RowCount) throw new ArgumentOutOfRangeException(nameof(startRow));
			if(startColumn < 0 || startColumn >= sheetInfo.ColumnCount) throw new ArgumentOutOfRangeException(nameof(startColumn));
			if(endRow < 0 || endRow >= sheetInfo.RowCount) throw new ArgumentOutOfRangeException(nameof(endRow));
			if(endColumn < 0 || endColumn >= sheetInfo.ColumnCount) throw new ArgumentOutOfRangeException(nameof(endColumn));
			
			_sheetInfo = sheetInfo;
			_startRow = startRow;
			_startColumn = startColumn;
			RowCount = endRow - startRow + 1;
			ColumnCount = endColumn - startColumn + 1;
		}

		public string? GetDataOrDefault(int row, int column) =>
			_sheetInfo.GetDataOrDefault(row + _startRow, column + _startColumn);
		public int GetRealRow(int row) => _startRow + row;
		public int GetRealColumn(int column) => _startColumn + column;
	}
}