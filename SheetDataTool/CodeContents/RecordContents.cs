using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Record", true)]
	internal sealed class RecordContents : NamedCodeContents<RecordContents.Element>
	{
		internal record Element()
		{
			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Type { get; init; } = string.Empty;

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, Comment : {Comment ?? "null"}";
			}
		}
		
		private readonly bool _isGlobal;

		public RecordContents( SheetInfoView sheetInfoView, Setting setting ) : base(sheetInfoView, setting)
		{
			_isGlobal = HasOption("Global", setting);
		}

		public override string ToString()
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Record");
			sb.AppendLine($"Name : {Name}");
			sb.AppendLine($"Options");
			sb.AppendLine($"Is global : {_isGlobal}");
			sb.AppendLine("Elements");
			Elements.ForEach(x => sb.AppendLine($"{x}"));
			return sb.ToString();
		}
	}
}
