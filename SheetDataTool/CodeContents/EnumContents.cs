using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Enum", true)]
	internal sealed class EnumContents : ElementCodeContents<EnumContents.Element>
	{
		private static readonly HashSet<string> UsableTypes
			= new() { "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong" };

		internal record Element()
		{
			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(false)]
			public string? Value { get; init; }

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }
			
			public void WriteScript(ScopedStringBuilder sb, Setting setting)
			{
				if (Comment is not null)
				{
					sb.WriteLine($"/// <summary> {Comment} </summary>");
				}

				sb.Write(Name.ChangeNotation(setting.InputNotation, setting.ScriptEnumItemNameNotation));
				if (Value is not null)
				{
					sb.Write($" = {Value}");
				}
				sb.WriteLine(",");
				sb.WriteLine();
			}

			public override string ToString()
			{
				return $"Name : {Name}, Value : {Value ?? "null"}, Comment : {Comment ?? "null"}";
			}
		}

		private readonly bool _isGlobal;
		private readonly string _type;
		private readonly string _name;
		private readonly string? _summary;

		public EnumContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			_isGlobal = HasOption("Global", setting);
			_type = UsableTypes.FirstOrDefault(x => HasOption(x)) ?? setting.EnumDefaultType;
			_name = GetName(sheetInfoView);
			_summary = GetSummary(sheetInfoView);
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization)
		{
			if (_isGlobal != isGlobal)
			{
				return;
			}

			WriteSummary(_summary, sb);
			sb.WriteLine("[JsonConverter(typeof(StringEnumConverter))]");
			using (sb.StartScope($"public enum {_name.ChangeNotation(setting.InputNotation, setting.ScriptEnumNameNotation)} : {_type}"))
			{
				Elements.ForEach(x => x.WriteScript(sb, setting));
			}

			sb.WriteLine();
		}

		public override string ToString() 
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Enum");
			sb.AppendLine($"Name : {_name}");
			sb.AppendLine($"Options");
			sb.AppendLine($"Is global : {_isGlobal}");
			sb.AppendLine($"Type : {_type}");
			sb.AppendLine("Elements");
			Elements.ForEach(x => sb.AppendLine($"{x}"));
			return sb.ToString();
		}
	}
}
