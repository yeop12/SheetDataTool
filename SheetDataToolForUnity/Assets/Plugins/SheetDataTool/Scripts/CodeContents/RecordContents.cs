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
			public string? Reference { get; init; }

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }
			
			public void WriteScript(ScopedStringBuilder sb, Setting setting)
			{
				if (Comment is not null)
				{
					sb.WriteLine($"/// <summary> {Comment} </summary>");
				}

				var publicName = setting.ToPublicVariableName(Name);
				var privateName = setting.ToPrivateVariableName(Name);

				var hasReference = string.IsNullOrWhiteSpace(Reference) is false;

				if (Type.StartsWith("List"))
				{
					sb.WriteLine($"[JsonProperty(nameof({publicName}))]");
					sb.WriteLine($"private {Type} {privateName} {{ get; init; }} = new();");
					sb.WriteLine();

					sb.WriteLine("[JsonIgnore]");
					sb.WriteLine($"public {Type.Replace("List", "IReadOnlyList")} {publicName} => {privateName};");
					sb.WriteLine();

					if (hasReference)
					{
						var referencePrivateName = privateName.Replace(setting.ReferenceReplacementSymbol,
							setting.ReferenceReplacementWord);
						var referencePublicName = publicName.Replace(setting.ReferenceReplacementSymbol,
							setting.ReferenceReplacementWord);
						sb.WriteLine("[JsonIgnore]");
						sb.WriteLine($"private List<{Reference}> {referencePrivateName};");
						sb.WriteLine();
						sb.WriteLine("[JsonIgnore]");
						sb.WriteLine($"public IReadOnlyList<{Reference}> {referencePublicName} => {referencePrivateName} ??= {publicName}.Select(x => {Reference}.Find(x)).ToList();");
						sb.WriteLine();
					}
				}
				else
				{
					sb.WriteLine($"public {Type} {publicName} {{ get; init; }}");
					sb.WriteLine();

					if (hasReference)
					{
						var referencePrivateName = privateName.Replace(setting.ReferenceReplacementSymbol,
							setting.ReferenceReplacementWord);
						var referencePublicName = publicName.Replace(setting.ReferenceReplacementSymbol,
							setting.ReferenceReplacementWord);
						sb.WriteLine("[JsonIgnore]");
						sb.WriteLine($"private {Reference} {referencePrivateName};");
						sb.WriteLine();
						sb.WriteLine("[JsonIgnore]");
						sb.WriteLine($"public {Reference} {referencePublicName} => {referencePrivateName} ??= {Reference}.Find({publicName});");
						sb.WriteLine();
					}
				}
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
			using (sb.StartScope($"public partial record {setting.ToRecordName(_name)}"))
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
