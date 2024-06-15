using System;
using UniEx.UI;

namespace SheetDataTool
{
    [AddressableInfo("UI/MessagePopup")]
    public class MessagePopupUI : FixedUIWindowTemplate<(string message, Action onClose)>
    {
	    public string Message => Model.message;

	    public override void Close(bool force = false)
	    {
		    base.Close(force);
			Model.onClose?.Invoke();
	    }
    }
}
