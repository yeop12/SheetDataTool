using ExcelSheetTool;

#if DEBUG
var arguments = Console.ReadLine()!.Split();
#else
var arguments = args;
#endif

if (arguments.Length == 0)
{
	Console.WriteLine($"Args must be contain {nameof(Command)} as first value.");
	return;
}

if (Enum.TryParse(arguments[0], out Command command) is false)
{
	Console.WriteLine($"{nameof(Command)} does not exist.");
	return;
}

if (arguments.Length < 1)
{
	Console.WriteLine($"Args must be contain excel sheet path as second value.");
	return;
}

var path = arguments[1];

try
{
	switch (command)
	{
		case Command.PrintSheetNames:
			var sheetNames = ExcelSheetUtil.GetSheetNames(path);
			Console.Write("Sheet names : ");
			Console.WriteLine(string.Join(',', sheetNames));
			break;

		case Command.PrintSheet:
			if (arguments.Length < 1) 
			{
				Console.WriteLine($"{nameof(Command.PrintSheet)} must be contain sheet name as third value.");
				return;
			}
			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			Console.WriteLine("Sheet info");
			Console.WriteLine(sheetInfo);
			break;

		default:
			throw new ArgumentOutOfRangeException();
	}
}
catch (Exception e) 
{
	Console.WriteLine(e);
}

public enum Command
{
	PrintSheetNames,
	PrintSheet,
}