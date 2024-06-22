using System;

namespace Sheet
{
	public class ExcelDataNotFoundException<T> : Exception
	{
		public Type DataType => typeof(T);
		public Object Key { get; private set; }
		public ExcelDataNotFoundException(object key) : base($"{typeof(T).Name} does not exist.(Key : {key})")
		{
			Key = key;
		}
	}
}
