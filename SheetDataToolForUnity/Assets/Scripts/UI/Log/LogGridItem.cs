using System;
using System.Diagnostics;
using System.IO;
using UniEx.UI;
using UnityEngine;
using Zenject;

namespace SheetDataTool
{
    public class LogGridItem : GridItemTemplate<Model.Log>
    {
	    [Inject] private Model.Tool _toolModel;

	    public string Title => $"{Model.SheetName}[{Model.Time:HH:mm:ss}]";
	    public string Message => Model.ToString();
	    public bool HasSheetReference => Model.Exception is ISheetReference;
	    public bool HasInformation => false;

	    public void OnShowSheetReference()
	    {
		    var sheetReference = (Model.Exception as ISheetReference)!;
		    var startInfo = new ProcessStartInfo
		    {
			    CreateNoWindow = true,
			    UseShellExecute = false,
			    FileName = $"{Application.streamingAssetsPath}/ExcelHelper.exe",
			    WindowStyle = ProcessWindowStyle.Hidden,
				ArgumentList = { "SelectCell", Path.GetFullPath(_toolModel.SheetUtil.GetPath(Model.SheetName)), Model.SheetName, $"{sheetReference.Row}", $"{sheetReference.Column}" },
		    };

		    try
		    {
			    using var exeProcess = Process.Start(startInfo);
		    }
		    catch(Exception e)
		    {
				_toolModel.Logs.Add(new Model.Log($"Select cell failed.({e})", Model.SheetName));
		    }
	    }

	    public void OnShowInformation()
	    {

	    }
    }
}
