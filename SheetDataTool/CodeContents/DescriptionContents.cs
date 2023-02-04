using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Description", false)]
	internal sealed class DescriptionContents : CodeContents
	{
		private readonly string _description;

		public DescriptionContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			_description = sheetInfoView[1, 0] ?? throw new InvalidSheetRuleException($"Description contents must be contain value at next row cell.", sheetInfoView.GetRealRow(1), sheetInfoView.GetRealColumn(0));
		}

		public override string ToString() 
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Description");
			sb.AppendLine($"Description : {_description}");
			return sb.ToString();
		}
	}
}
