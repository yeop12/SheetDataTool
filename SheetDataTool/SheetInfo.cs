using System;
using System.Linq;
using System.Text;

namespace SheetDataTool
{
	public sealed class SheetInfo
	{
		private readonly string?[,] _data;

		public string Name { get; }

		public string? this[int row, int column] => _data[row, column];
		public int RowCount => _data.GetLength(0);
		public int ColumnCount => _data.GetLength(1);

		public SheetInfo(string name, string?[,] data)
		{
			Name = name;
			_data = data;
		}

		public string? GetDataOrDefault(int row, int column)
		{
			if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount) return null;
			return _data[row, column];
		}

		public int FindRow( int startRow, int column, Predicate<string?> match )
		{
			if (startRow < 0) throw new ArgumentOutOfRangeException(nameof(startRow));
			if (column < 0 || column >= ColumnCount) throw new ArgumentOutOfRangeException(nameof(column));
			if (match is null) throw new ArgumentNullException(nameof(match));
			
			for (var row = startRow; row < RowCount; ++row)
			{
				var cell = this[row, column];
				if (match.Invoke(cell))
				{
					return row;
				}
			}

			return -1;
		}

		public override string ToString()
		{
			var sb = new StringBuilder(RowCount * ColumnCount * 10);
			sb.AppendLine($"Name : {Name}");
			sb.AppendLine($"Row count : {RowCount}, Column : {ColumnCount}");
			var maxLength = _data.Cast<string>().Where(x => x is not null).Max(x => x.Length + (Encoding.UTF8.GetByteCount(x) - x.Length) / 2);
			var line = new string('-', (maxLength + 1) * ColumnCount);
			for (var row = 0; row < RowCount; ++row)
			{
				sb.AppendLine(line);
				for (var column = 0; column < ColumnCount; ++column)
				{
					var data = _data[row, column] ?? string.Empty;
					var padSize = maxLength - (Encoding.UTF8.GetByteCount(data) - data.Length) / 2;
					sb.Append(data.PadLeft(padSize, ' '));
					sb.Append('|');
				}
				sb.AppendLine();
			}
			sb.AppendLine(line);
			return sb.ToString();
		}
	}
}