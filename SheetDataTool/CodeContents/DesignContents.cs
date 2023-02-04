using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Design", false)]
	internal class DesignContents : ElementCodeContents<DesignContents.Element>
	{
		internal record Element()
		{
			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Type { get; init; } = string.Empty;

			[ContentsElementItemDescription(true, true)]
			public bool IsPrimaryKey { get; init; }

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, IsPrimaryKey : {IsPrimaryKey}, Comment : {Comment ?? "null"}";
			}
		}

		public DesignContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
		}

		public override string ToString()
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Design");
			sb.AppendLine("Elements");
			Elements.ForEach(x => sb.AppendLine($"{x}"));
			return sb.ToString();
		}
	}
}
