using ExcelHelper;

#if DEBUG
var values = Console.ReadLine()?.Split();
#else
var values = args;
#endif

if (values is null || values.Length < 1)
{
	return;
}

if (Enum.TryParse(values[0], out Command command) is false)
{
	return;
}

switch (command)
{
	case Command.OpenSheet:
	{
		if (values.Length < 3)
		{
			return;
		}

		var path = values[1];
		var sheetName = values[2];
		using var sheetUtil = new ExcelUtil(path, sheetName);
		sheetUtil.ActivateSheet();
	}
		break;

	case Command.SelectCell:
	{
		if (values.Length < 5)
		{
			return;
		}

		var path = values[1];
		var sheetName = values[2];
		var row = int.Parse(values[3]);
		var column = int.Parse(values[4]);
		using var sheetUtil = new ExcelUtil(path, sheetName);
		sheetUtil.SelectCell(row, column);
	}
		break;

	default:
		throw new NotImplementedException();
}

internal enum Command
{
	OpenSheet,
	SelectCell,
}