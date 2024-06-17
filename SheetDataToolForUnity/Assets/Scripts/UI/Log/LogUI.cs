using UniEx;
using UniEx.UI;
using UniRx;

namespace SheetDataTool
{
    [AddressableInfo("UI/Log")]
    public class LogUI : FixedUIWindowTemplate<ReactiveCollection<Model.Log>>
    {
	    public ReadOnlyReactiveCollectionWrapper<Model.Log> Logs => new(Model);
    }
}
