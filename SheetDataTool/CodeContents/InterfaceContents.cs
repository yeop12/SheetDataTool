using System.Text;

namespace SheetDataTool
{
	[ContentsDescription("Interface", true)]
	internal sealed class InterfaceContents : CodeContents
	{
		private readonly string _name;
		private readonly string? _summary;

		public InterfaceContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			_name = GetName(sheetInfoView);
			_summary = GetSummary(sheetInfoView);
		}

		public override void WriteScript(ScopedStringBuilder sb, bool isGlobal, Setting setting, bool madeForSerialization )
		{
			WriteSummary(_summary, sb);
			using (sb.StartScope($"public partial interface {setting.ScriptInterfaceNamePrefix}{_name.ChangeNotation(setting.InputNotation, setting.ScriptInterfaceNameNotation)}"))
			{
			}

			sb.WriteLine();
		}

		public override string ToString()
		{
			var sb = new StringBuilder(200);
			sb.AppendLine("Contents type : Interface");
			sb.AppendLine($"Name : {_name}");
			return sb.ToString();
		}
	}
}
