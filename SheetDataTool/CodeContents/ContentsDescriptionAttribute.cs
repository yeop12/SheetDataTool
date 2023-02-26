using System;

namespace SheetDataTool
{
	[AttributeUsage(AttributeTargets.Class)]
	internal sealed class ContentsDescriptionAttribute : Attribute
	{
		public string Name { get; }
		public bool CanRegisterMultiple { get; }

		public ContentsDescriptionAttribute( string name, bool canRegisterMultiple ) 
		{
			Name = name;
			CanRegisterMultiple = canRegisterMultiple;
		}
	}
}