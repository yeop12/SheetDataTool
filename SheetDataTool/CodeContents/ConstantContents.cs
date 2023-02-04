using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Constant", false)]
	internal sealed class ConstantContents : ElementCodeContents<ConstantContents.Element>
	{
		internal record Element()
		{
			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Type { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Value { get; init; } = string.Empty;

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, Value : {Value}, Comment : {Comment ?? "null"}";
			}
		}

		public ConstantContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
		}

		public override string ToString()
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Constant");
			sb.AppendLine("Elements");
			Elements.ForEach(x => sb.AppendLine($"{x}"));
			return sb.ToString();
		}
	}
}
