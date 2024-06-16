using System;
using System.Text;

namespace SheetDataTool
{
	internal sealed class ScopedStringBuilder
	{
		private readonly StringBuilder _builder;
		private int _tabCount;
		private string _tabString = string.Empty;
		private bool _writeTab = true;

		public ScopedStringBuilder( int capacity = 2000 )
		{
			_builder = new StringBuilder(capacity);
		}

		public class Scope : IDisposable
		{
			private readonly ScopedStringBuilder _builder;

			public Scope( ScopedStringBuilder builder )
			{
				_builder = builder;
			}

			public void Dispose()
			{
				_builder.EndScope();
			}
		}

		public Scope StartScope(string value)
		{
			WriteLine(value);
			WriteLine("{");
			++_tabCount;
			_tabString = new string('\t', _tabCount);
			return new Scope(this);
		}

		private void EndScope() 
		{
			--_tabCount;
			_tabString = new string('\t', _tabCount);
			WriteLine("}");
		}

		public void Write(string value) 
		{
			if (_writeTab)
			{
				_builder.Append(_tabString);
				_writeTab = false;
			}
			_builder.Append(value);
		}

		public void WriteLine() 
		{
			_builder.AppendLine();
			_writeTab = true;
		}

		public void WriteLine(string value) 
		{
			if (_writeTab)
			{
				_builder.Append(_tabString);
			}
			_builder.AppendLine(value);
			_writeTab = true;
		}

		public override string ToString()
		{
			return _builder.ToString();
		}
	}
}
