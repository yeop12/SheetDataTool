using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Design", false)]
	internal sealed class DesignContents : ElementCodeContents<DesignContents.Element>
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

			public void WriteScript(ScopedStringBuilder sb, Setting setting, bool madeForSerialization )
			{
				var publicName = Name.ChangeNotation(setting.InputNotation, setting.ScriptPublicVariableNameNotation);
				var privateName = $"{setting.ScriptPrivateVariableNamePrefix}{Name.ChangeNotation(setting.InputNotation, setting.ScriptPrivateVariableNameNotation)}";
				
				if (Comment is not null)
				{
					sb.WriteLine($"/// <summary> {Comment} </summary>");
				}
				
				if (Type.StartsWith("List"))
				{
					sb.WriteLine($"[JsonProperty(nameof({publicName}))]");
					sb.WriteLine($"public {Type} {privateName} {{ get; init; }}");
					sb.WriteLine();

					if (madeForSerialization)
					{
						sb.WriteLine($"public bool ShouldSerialize{privateName}() => _serializeDesign;");
					}

					sb.WriteLine("[JsonIgnore]");
					sb.WriteLine($"public {Type.Replace("List", "IReadOnlyList")} {publicName} => {privateName};");
				}
				else
				{
					sb.WriteLine($"public {Type} {publicName} {{ get; init; }}");

					if (madeForSerialization)
					{
						sb.WriteLine($"public bool ShouldSerialize{publicName}() => _serializeDesign;");
					}
				}

				sb.WriteLine();
			}

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, IsPrimaryKey : {IsPrimaryKey}, Comment : {Comment ?? "null"}";
			}
		}

		public string KeyType { get; }
		public string KeyName { get; }
		public readonly List<string>? InheritedInterfaceNames;

		public DesignContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			var primaryKeyElements = Elements.Where(x => x.IsPrimaryKey).ToList();
			KeyType = primaryKeyElements.Count == 1
				? primaryKeyElements.First().Type
				: $"({primaryKeyElements.Select(x => x.Type).Aggregate((x, y) => $"{x}, {y}")})";
			KeyName = primaryKeyElements.Count == 1 
				? primaryKeyElements.First().Name 
				: $"({primaryKeyElements.Select(x => x.Name).Aggregate(( x, y ) => $"{x}, {y}")})";
			InheritedInterfaceNames = sheetInfoView[0, 1]?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.ChangeNotation(setting.InputNotation, setting.ScriptClassNameNotation)).ToList();
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization )
		{
			sb.WriteLine("[JsonIgnore]");
			sb.WriteLine($"public {KeyType} {"Key".ChangeNotation(Notation.Pascal, setting.ScriptPublicVariableNameNotation)} => {KeyName.ChangeNotation(setting.InputNotation, setting.ScriptPublicVariableNameNotation)};");
			sb.WriteLine();
			Elements.ForEach(x => x.WriteScript(sb, setting, madeForSerialization));
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
