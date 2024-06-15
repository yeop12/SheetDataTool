using System;

namespace SheetDataTool.Model
{
    public class Log
    {
        public DateTime Time { get; } = DateTime.Now;
        public string Message { get; }
        public string SheetName { get; }
        public Exception Exception { get; }

        public Log(Exception exception, string sheetName)
        {
            Exception = exception;
            SheetName = sheetName;
        }

        public Log(string message, string sheetName)
        {
            Message = message;
            SheetName = sheetName;
        }

        public override string ToString()
        {
	        return Exception?.ToString() ?? Message;
        }
    }
}
