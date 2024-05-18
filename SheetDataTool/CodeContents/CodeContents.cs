using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SheetDataTool
{
	internal abstract class CodeContents
	{
		private static readonly char[] OptionSeparators = { '[', ']', ':' };

		public static bool IsContentsCell([NotNullWhen(returnValue: true)] string? value ) => value is { Length: > 0 } && value[0] == '[';
		
		public static string? GetContentsTypeName( string value ) 
		{
			var options = value.Split(OptionSeparators, StringSplitOptions.RemoveEmptyEntries);
			return options is { Length: > 0 } ? options[0] : null;
		}

		private static List<string> GetOptions( SheetInfoView sheetInfoView ) 
		{
			var flagCell = sheetInfoView[0, 0];
			if (flagCell is null) 
			{
				throw new Exception("The first cell of CodeContents cannot be null.");
			}

			return flagCell.Split(OptionSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
		}

		protected static string GetName(SheetInfoView sheetInfoView)
		{
			return sheetInfoView[0, 1] ?? throw new NotExistContentsNameException(GetContentsTypeName(sheetInfoView[0, 0]!)!, sheetInfoView.GetRealRow(0), sheetInfoView.GetRealColumn(1));
		}

		protected static string? GetSummary(SheetInfoView sheetInfoView)
		{
			return sheetInfoView.GetDataOrDefault(0, 2);
		}

		protected static void WriteSummary(string? summary, ScopedStringBuilder sb)
		{
			if (summary is null) return;

			sb.WriteLine("/// <summary>");
			sb.WriteLine($"/// {summary}");
			sb.WriteLine("/// </summary>");
		}

		protected List<string> Options { get; }

		protected CodeContents( SheetInfoView sheetInfoView, Setting setting )
		{
			Options = GetOptions(sheetInfoView);
		}

		protected bool HasOption(string optionName, Setting? setting = null) => Options.Contains(setting is null ? optionName : optionName.ChangeNotation(Notation.Pascal, setting.InputNotation));

		public abstract void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization);
	}
}
