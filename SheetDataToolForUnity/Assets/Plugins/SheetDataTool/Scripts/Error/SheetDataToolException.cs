using System;

namespace SheetDataTool
{
	public abstract class SheetDataToolException : Exception
	{
		protected SheetDataToolException()
		{

		}

		protected SheetDataToolException(string message) : base(message)
		{

		}
	}
}
