﻿namespace SheetDataTool
{
	internal static class LinqExtension
	{
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var item in enumerable)
			{
				action.Invoke(item);
			}
		}
	}
}
