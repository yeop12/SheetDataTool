using System.Runtime.CompilerServices;

namespace SheetDataTool
{
	[AttributeUsage(AttributeTargets.Property)]
	internal sealed class ContentsElementItemDescriptionAttribute : Attribute
	{
		public string Name { get; }
		public bool IsEssential { get; }
		public bool AllowsEmpty { get; }

		public ContentsElementItemDescriptionAttribute( bool isEssential, bool allowsEmpty = false, [CallerMemberName] string name = "" )
		{
			Name = name;
			IsEssential = isEssential;
			AllowsEmpty = allowsEmpty;
		}
	}
}
