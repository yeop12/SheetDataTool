using System.Text.RegularExpressions;

namespace SheetDataTool
{
	internal static class NotationUtil
	{
		public static string ChangeNotation( this string text, Notation originalNotation, Notation targetNotation ) 
		{
			if(originalNotation == targetNotation) 
			{
				return text;
			}
			else
				return originalNotation switch
				{
					Notation.UpperSnake when targetNotation == Notation.LowerSnake => text.ToLower(),
					Notation.LowerSnake when targetNotation == Notation.UpperSnake => text.ToUpper(),
					Notation.Camel when targetNotation == Notation.Pascal => text.ToUpper(0),
					Notation.Pascal when targetNotation == Notation.Camel => text.ToLower(0),
					_ => TokensToText(TextToTokens(text, originalNotation), targetNotation)
				};
		}

		private static IEnumerable<string> TextToTokens(string text, Notation notation)
		{
			var evaluator = new MatchEvaluator(x => $"_{x.Value.ToLower()}");

			var lowerSnakeText =  notation switch 
			{
				Notation.LowerSnake => text,
				Notation.UpperSnake => text.ToLower(),
				Notation.Pascal => Regex.Replace(text, "[A-Z]", evaluator),
				Notation.Camel => Regex.Replace(text, "[A-Z]", evaluator),
				_ => throw new ArgumentException(nameof(notation))
			};
			return lowerSnakeText.Split('_', StringSplitOptions.RemoveEmptyEntries);
		}

		private static string TokensToText(IEnumerable<string> tokens, Notation notation)
		{
			return notation switch
			{
				Notation.LowerSnake => string.Join('_', tokens),
				Notation.UpperSnake => string.Join('_', tokens).ToUpper(),
				Notation.Camel => string.Concat(tokens.Select(x => x.ToUpper(0))).ToLower(0),
				Notation.Pascal => string.Concat(tokens.Select(x => x.ToUpper(0))),
				_ => throw new ArgumentException(nameof(notation))
			};
		}

		private static string ToUpper(this string text, int index)
		{
			return $"{text[..( index + 1 )].ToUpper()}{text[(index + 1)..]}";
		}

		private static string ToLower( this string text, int index ) {
			return $"{text[..( index + 1 )].ToLower()}{text[( index + 1 )..]}";
		}
	}
}
