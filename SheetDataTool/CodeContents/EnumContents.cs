using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Enum", true)]
	internal sealed class EnumContents : NamedCodeContents<EnumContents.Element>
	{
		private static readonly HashSet<string> UsableTypes
			= new() { "Byte", "Sbyte", "Short", "Ushort", "Int", "Uint", "Long", "Ulong" };

		internal record Element()
		{
			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(false)]
			public string? Value { get; init; }

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }

			public override string ToString()
			{
				return $"Name : {Name}, Value : {Value ?? "null"}, Comment : {Comment ?? "null"}";
			}
		}

		private readonly bool _isGlobal;
		private readonly string _type;

		public EnumContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			_isGlobal = HasOption("Global", setting);
			_type = UsableTypes.FirstOrDefault(x => HasOption(x, setting)) ?? setting.EnumDefaultType;
		}

		public override string ToString() 
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Enum");
			sb.AppendLine($"Name : {Name}");
			sb.AppendLine($"Options");
			sb.AppendLine($"Is global : {_isGlobal}");
			sb.AppendLine($"Type : {_type}");
			sb.AppendLine("Elements");
			Elements.ForEach(x => sb.AppendLine($"{x}"));
			return sb.ToString();
		}
	}
}
