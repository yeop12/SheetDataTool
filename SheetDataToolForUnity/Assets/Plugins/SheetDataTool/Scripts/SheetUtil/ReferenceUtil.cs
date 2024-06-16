using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SheetDataTool
{
	public static class ReferenceUtil
	{
		public static (int rowIndex, int columnIndex) ParseReference( string text ) 
		{
			if (text == null) 
			{
				throw new ArgumentNullException(nameof(text));
			}

			var match = Regex.Match(text, @"([a-zA-Z]+)(\d+)");
			if (!match.Success) 
			{
				throw new FormatException(nameof(text));
			}

			var columnText = match.Groups[1].Value;
			var rowText = match.Groups[2].Value;
			var row = int.Parse(rowText);
			const int alphabetCount = 'Z' - 'A' + 1;
			var column = columnText.Aggregate(0, ( current, t ) => ( current * alphabetCount ) + ( (int)t - 'A' + 1 ));
			return (row - 1, column - 1);
		}

		public static string GetReference(int row, int column) => $"{GetColumnReference(column)}{GetRowReference(row)}";

		public static string GetRowReference(int row) => $"{row + 1}";

		public static string GetColumnReference(int column)
		{
			var result = new StringBuilder();
			const int alphabetCount = 'Z' - 'A' + 1;
			do
			{
				var alphabet = (char)('A' + (column % alphabetCount));
				result.Append(alphabet);
				column /= alphabetCount;
			} while (column != 0);

			return new string(result.ToString().Reverse().ToArray());
		}
	}
}
