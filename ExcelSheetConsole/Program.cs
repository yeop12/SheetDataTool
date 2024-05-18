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

var setting = new Setting();
setting.PlatformInfos.Add(new PlatformInfo() 
{
	Platform = Platform.CSharp,
	DefineName = "Server",
});

try
{
	var sheetUtil = new ExcelSheetUtil(path);

	switch (command)
	{
		case Command.PrintSheetNames:
		{
			Console.Write("Sheet names : ");
			Console.WriteLine(string.Join(',', sheetUtil.GetSheetNames()));
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
			var sheetInfo = sheetUtil.GetSheetInfo(sheetName);
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
			var sheetInfo = sheetUtil.GetSheetInfo(sheetName);
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
			var sheetInfo = sheetUtil.GetSheetInfo(sheetName);
			var contentsData = new ContentsData(sheetInfo, setting);
			Console.WriteLine(contentsData.GetScript(false));
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
			var sheetInfo = sheetUtil.GetSheetInfo(sheetName);
			var contentsData = new ContentsData(sheetInfo, setting);
			const string assemblyName = "TempLib";
			var scripts = new[]
			{
				contentsData.GetScript(true),
				ScriptUtil.GetBaseClassScript(setting),
				ScriptUtil.GetDesignInterfaceScript(setting),
				ScriptUtil.GetDesignClassScript(setting),
				ScriptUtil.GetConstantClassScript(setting),
				ScriptUtil.GetFullClassScript(setting),
				ScriptUtil.GetExcelDataNotFoundExceptionScript(setting),
				ScriptUtil.GetUnityTypeScript(),
			};
			CompileUtil.Compile(assemblyName, scripts);
			Console.WriteLine("Compilation success.");
		}
			break;

		case Command.PrintBaseScript:
		{
			Console.WriteLine(ScriptUtil.GetBaseClassScript(setting));
		}
			break;

		case Command.PrintDesignInterfaceScript:
		{
			Console.WriteLine(ScriptUtil.GetDesignInterfaceScript(setting));
		}
			break;

		case Command.PrintDesignClassScript:
		{
			Console.WriteLine(ScriptUtil.GetDesignClassScript(setting));
		}
			break;

		case Command.PrintConstantClassScript:
		{
			Console.WriteLine(ScriptUtil.GetConstantClassScript(setting));
		}
			break;

		case Command.PrintFullClassScript:
		{
			Console.WriteLine(ScriptUtil.GetFullClassScript(new Setting()));
		}
			break;

		case Command.PrintUnityTypeScript:
		{
			Console.WriteLine(ScriptUtil.GetUnityTypeScript());
		}
			break;

		case Command.PrintSerializedObject:
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine($"{nameof(Command.PrintSerializedObject)} must be contain sheet name as third value.");
				return;
			}

			var sheetName = arguments[2];
			var sheetInfo = sheetUtil.GetSheetInfo(sheetName);
			var contentsData = new ContentsData(sheetInfo, setting);
			const string assemblyName = "TempLib";
			var scripts = new[]
			{
				contentsData.GetScript(true),
				ScriptUtil.GetBaseClassScript(setting),
				ScriptUtil.GetDesignInterfaceScript(setting),
				ScriptUtil.GetDesignClassScript(setting),
				ScriptUtil.GetConstantClassScript(setting),
				ScriptUtil.GetFullClassScript(setting),
				ScriptUtil.GetExcelDataNotFoundExceptionScript(setting),
				ScriptUtil.GetUnityTypeScript(),
			};
			var assembly = CompileUtil.Compile(assemblyName, scripts);
			Console.WriteLine(contentsData.Serialize(assembly));
		}
			break;

		default:
			throw new ArgumentOutOfRangeException();
	}
}
catch (SheetDataToolException e)
{
	Console.WriteLine(e);
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
	PrintBaseScript,
	PrintDesignInterfaceScript,
	PrintDesignClassScript,
	PrintConstantClassScript,
	PrintFullClassScript,
	PrintUnityTypeScript,
	PrintSerializedObject,
}