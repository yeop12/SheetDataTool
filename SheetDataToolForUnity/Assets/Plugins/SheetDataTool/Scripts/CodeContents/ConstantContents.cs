using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace SheetDataTool
{
	[ContentsDescription("Constant", false)]
	internal sealed class ConstantContents : ElementCodeContents<ConstantContents.Element>
	{
		internal record Element()
		{
			public int Row { get; init; }

			[ContentsElementItemDescription(true)]
			public string Name { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Type { get; init; } = string.Empty;

			[ContentsElementItemDescription(true)]
			public string Value { get; init; } = string.Empty;

			public int ValueColumn { get; init; }

			[ContentsElementItemDescription(false)]
			public string? Comment { get; init; }

			public void WriteScript(ScopedStringBuilder sb, Setting setting, bool madeForSerialization )
			{
				var privateItemName = setting.ToPrivateVariableName(Name);
				var publicItemName = setting.ToPublicVariableName(Name);

				if (madeForSerialization)
				{
					sb.WriteLine($"public static bool ShouldSerialize{privateItemName}() => _serializeDesign is false;");
				}

				sb.WriteLine($"[JsonProperty(nameof({publicItemName}))]");
				sb.WriteLine($"private static {Type} {privateItemName} {{ get; set; }}");
				sb.WriteLine();

				if (Comment is not null)
				{
					sb.WriteLine($"/// <summary> {Comment} </summary>");
				}
				sb.WriteLine("[JsonIgnore]");
				using (sb.StartScope($"public static {Type} {publicItemName}"))
				{
					using (sb.StartScope("get"))
					{
						var loadFunctionName = setting.PascalToFunctionName("LoadData");
						sb.WriteLine($"if(IsLoaded is false) {loadFunctionName}();");
						sb.WriteLine($"return {privateItemName};");
					}
				}
				sb.WriteLine();
			}

			public void SetData(object obj, Setting setting) 
			{
				var privateItemName = setting.ToPrivateVariableName(Name);
				var propertyInfo = obj.GetType().GetProperty(privateItemName, BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic);
				if (propertyInfo is null) throw new Exception($"{obj.GetType()} type does not contain {privateItemName} property.");
				if (TypeUtil.IsBasicType(Type))
				{
					try
					{
						var value = TypeUtil.ChangeType(Value, propertyInfo.PropertyType);
						propertyInfo.SetValue(obj, value);
					}
					catch
					{
						throw new MismatchTypeException(propertyInfo.PropertyType, Value, Row, ValueColumn);
					}
				}
				else
				{
					var value = JsonConvert.DeserializeObject(Value, propertyInfo.PropertyType);
					propertyInfo.SetValue(obj, value);
				}
			}

			public override string ToString()
			{
				return $"Name : {Name}, Type : {Type}, Value : {Value}, Comment : {Comment ?? "null"}";
			}
		}

		public ConstantContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization)
		{
			Elements.ForEach(x => x.WriteScript(sb, setting, madeForSerialization));
			sb.WriteLine();
		}

		public void SetData(object obj, Setting setting)
		{
			Elements.ForEach(x => x.SetData(obj, setting));
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
