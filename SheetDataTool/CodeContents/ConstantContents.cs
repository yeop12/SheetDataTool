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

			public void WriteScript(ScopedStringBuilder sb, Setting setting)
			{
				if (Comment is not null)
				{
					sb.WriteLine($"/// <summary> {Comment} </summary>");
				}
				sb.WriteLine($"public static {Type} {Name.ChangeNotation(setting.InputNotation, setting.ScriptConstantPropertyNameNotation)} {{ get; private set; }}");
				sb.WriteLine();
			}

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, Value : {Value}, Comment : {Comment ?? "null"}";
			}
		}

		public ConstantContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting)
		{
			Elements.ForEach(x => x.WriteScript(sb, setting));
			sb.WriteLine();
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
