using System;
using System.IO;

namespace SheetDataTool
{
    public record AccessInfo
    {
	    public SheetType SheetType { get; set; }
	    public string Path { get; set; } = string.Empty;
	    public string GoogleSheetId { get; set; } = string.Empty;

	    public SheetUtil GetSheetUtil()
	    {
		    return SheetType switch
		    {
			    SheetType.ExcelSheet => new ExcelSheetUtil(Path),
			    SheetType.GoogleSheet => new GoogleSheetUtil(Path, GoogleSheetId),
			    _ => throw new NotImplementedException(),
		    };
	    }

	    public bool IsValid()
	    {
		    if (SheetType == SheetType.ExcelSheet)
		    {
			    if (string.IsNullOrWhiteSpace(Path) || Directory.Exists(Path) is false)
			    {
				    return false;
			    }
		    }

		    return true;
	    }
    }
}
