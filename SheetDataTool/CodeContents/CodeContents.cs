using System.Diagnostics.CodeAnalysis;

namespace SheetDataTool
{
	internal abstract class CodeContents
	{
		private static readonly char[] OptionSeparators = { '[', ']', ':' };

		public static bool IsContentsCell([NotNullWhen(returnValue: true)] string? value ) => value is { Length: > 0 } && value[0] == '[';
		
		public static string? GetContentsName( string value ) 
		{
			var options = value.Split(OptionSeparators, StringSplitOptions.RemoveEmptyEntries);
			return options is { Length: > 0 } ? options[0] : null;
		}

		private static List<string> GetOptions( SheetInfoView sheetInfoView ) 
		{
			var flagCell = sheetInfoView[0, 0];
			if (flagCell is null) 
			{
				throw new InvalidSheetRuleException("The first cell of CodeContents cannot be null.", sheetInfoView.GetRealRow(0), sheetInfoView.GetRealColumn(0));
			}

			return flagCell.Split(OptionSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
		}

		protected List<string> Options { get; }

		protected CodeContents( SheetInfoView sheetInfoView, Setting setting )
		{
			Options = GetOptions(sheetInfoView);
		}

		protected bool HasOption(string optionName, Setting setting) => Options.Contains(optionName.ChangeNotation(Notation.Pascal, setting.InputNotation));
	}
}
