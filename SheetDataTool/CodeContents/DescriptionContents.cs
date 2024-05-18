using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Description", false)]
	internal sealed class DescriptionContents : CodeContents
	{
		private readonly string _description;

		public DescriptionContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			_description = sheetInfoView[1, 0] ?? throw new NotExistDescriptionValueException(sheetInfoView.GetRealRow(0));
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization)
		{
			sb.WriteLine("/// <summary>");
			sb.WriteLine($"/// {_description}");
			sb.WriteLine("/// </summary>");
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
