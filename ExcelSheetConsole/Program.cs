using ExcelSheetTool;
using SheetDataTool;

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
		{
			var sheetNames = ExcelSheetUtil.GetSheetNames(path);
			Console.Write("Sheet names : ");
			Console.WriteLine(string.Join(',', sheetNames));
		}
			break;

		case Command.PrintSheet:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.PrintSheet)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			Console.WriteLine("Sheet info");
			Console.WriteLine(sheetInfo);
		}
			break;

		case Command.PrintContentsData:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.PrintContentsData)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			var setting = new Setting();
			var contentsData = new ContentsData(sheetInfo, setting);
			Console.WriteLine("Contents data");
			Console.WriteLine(contentsData);
		}
			break;

		case Command.PrintScript:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.PrintScript)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			var setting = new Setting();
			var contentsData = new ContentsData(sheetInfo, setting);
			Console.WriteLine("Script");
			Console.WriteLine(contentsData.GetScript());
		}
			break;

		case Command.CompileScript:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.CompileScript)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			var setting = new Setting();
			var contentsData = new ContentsData(sheetInfo, setting);
			contentsData.CompileScript();
			Console.WriteLine("Compilation success.");
		}
			break;

		case Command.PrintSheetDataInterfaceScript:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.PrintSheetDataInterfaceScript)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			var setting = new Setting();
			var contentsData = new ContentsData(sheetInfo, setting);
			Console.WriteLine(contentsData.GetSheetDataInterfaceScript());
		}
			break;

		case Command.PrintSheetDataScript:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.PrintSheetDataScript)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = ExcelSheetUtil.GetSheetInfo(path, sheetName);
			var setting = new Setting();
			var contentsData = new ContentsData(sheetInfo, setting);
			Console.WriteLine(contentsData.GetSheetDataRecordScript());
		}
			break;

		case Command.PrintUnityTypeScript:
		{
			Console.WriteLine(ContentsData.GetUnityTypeScript());
		}
			break;

		default:
			throw new ArgumentOutOfRangeException();
	}
}
catch (InvalidSheetRuleException e)
{
	Console.WriteLine(e);
	if (e.Row is not null && e.Column is not null)
	{
		Console.WriteLine($"Reference : {ExcelSheetUtil.GetReference(e.Row.Value, e.Column.Value)}");
	}
	else if (e.Row is not null)
	{
		Console.WriteLine($"Row reference : {ExcelSheetUtil.GetRowReference(e.Row.Value)}");
	}
	else if (e.Column is not null)
	{
		Console.WriteLine($"Column Reference : {ExcelSheetUtil.GetColumnReference(e.Column.Value)}");
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
	PrintContentsData,
	PrintScript,
	CompileScript,
	PrintSheetDataInterfaceScript,
	PrintSheetDataScript,
	PrintUnityTypeScript,
}