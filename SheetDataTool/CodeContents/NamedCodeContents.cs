namespace SheetDataTool
{
	internal abstract class NamedCodeContents<T> : ElementCodeContents<T>
	{
		protected string Name { get; }

		protected NamedCodeContents(SheetInfoView sheetInfoView, Setting setting ) : base(sheetInfoView, setting)
		{
			Name = GetName(sheetInfoView);
		}

		private static string GetName(SheetInfoView sheetInfoView)
		{
			return sheetInfoView[0, 1] ?? throw new InvalidSheetRuleException("NamedCodeContents second cell must not be null.", sheetInfoView.GetRealRow(0), sheetInfoView.GetRealColumn(1));
		}
	}
}
