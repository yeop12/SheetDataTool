using UniEx.UI;

namespace SheetDataTool
{
    public class LogGridItem : GridItemTemplate<Model.Log>
    {
	    public string Title => $"{Model.SheetName}[{Model.Time:HH:mm:ss}]";
	    public string Message => Model.ToString();
	    public bool HasSheetReference => false;
	    public bool HasInformation => false;

	    public void OnShowSheetReference()
	    {

	    }

	    public void OnShowInformation()
	    {

	    }
    }
}
