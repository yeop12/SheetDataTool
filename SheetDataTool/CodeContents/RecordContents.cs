using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Record", true)]
	internal sealed class RecordContents : ElementCodeContents<RecordContents.Element>
	{
		internal record Element()
		{
			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Type { get; init; } = string.Empty;

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }
			
			public void WriteScript(ScopedStringBuilder sb, Setting setting)
			{
				if (Comment is not null)
				{
					sb.WriteLine($"/// <summary> {Comment} </summary>");
				}

				sb.WriteLine($"public {Type} {Name.ChangeNotation(setting.InputNotation, setting.ScriptRecordPropertyNameNotation)} {{ get; init; }}");
				sb.WriteLine();
			}

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, Comment : {Comment ?? "null"}";
			}
		}
		
		private readonly bool _isGlobal;
		private readonly string _name;
		private readonly string? _summary;

		public RecordContents( SheetInfoView sheetInfoView, Setting setting ) : base(sheetInfoView, setting)
		{
			_isGlobal = HasOption("Global", setting);
			_name = GetName(sheetInfoView);
			_summary = GetSummary(sheetInfoView);
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization )
		{
			if (_isGlobal != isGlobal)
			{
				return;
			}
			
			WriteSummary(_summary, sb);
			sb.WriteLine("[Serializable]");
			using (sb.StartScope($"public partial record {_name.ChangeNotation(setting.InputNotation, setting.ScriptRecordNameNotation)}"))
			{
				Elements.ForEach(x => x.WriteScript(sb, setting));
			}

			sb.WriteLine();
		}

		public override string ToString()
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Record");
			sb.AppendLine($"Name : {_name}");
			sb.AppendLine($"Options");
			sb.AppendLine($"Is global : {_isGlobal}");
			sb.AppendLine("Elements");
			Elements.ForEach(x => sb.AppendLine($"{x}"));
			return sb.ToString();
		}
	}
}
