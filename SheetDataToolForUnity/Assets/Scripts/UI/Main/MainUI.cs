using System;
using SheetDataTool.Model;
using UniEx;
using UniEx.UI;
using UniRx;
using Zenject;

namespace SheetDataTool
{
    [AddressableInfo("UI/Main")]
    public class MainUI : FixedUIWindowTemplate<Model.Tool>
    {
	    [Inject] private FixedUIManager _fixedUIManager;

	    private ReactiveProperty<string> _refreshAnimatorTrigger;

	    public IObservable<bool> ShowLogMark => Model.LogCount.Select(x => x != 0);
	    public IObservable<string> LogCountText => Model.LogCount.Select(x => $"{x}");
	    public IObservable<bool> ShowBookMarkSheets =>
		    Model.WholeBookMarkSheetNames.ObserveCountChanged(true).Select(x => x != 0);
	    public ReadOnlyReactiveCollectionWrapper<string> BookMarkSheetNames => new(Model.VisibleBookMarkSheetNames);
        public ReadOnlyReactiveCollectionWrapper<string> NormalSheetNames => new(Model.VisibleNormalSheetNames);
        public string SearchText
        {
	        set => Model.SearchText.Value = value;
        }
        public IObservable<string> RefreshAnimatorTrigger => _refreshAnimatorTrigger;

        protected override void OnOpen(Tool model)
        {
	        base.OnOpen(model);
	        _refreshAnimatorTrigger = new ReactiveProperty<string>();
        }

        public void OnOpenMenuUI()
        {
	        _fixedUIManager.Open<MenuUI>(Model);
        }

        public void OnOpenLogUI()
        {
	        Model.LogCount.Value = 0;
	        _fixedUIManager.Open<LogUI>(Model.Logs);
        }

        public void OnRefresh()
        {
	        _refreshAnimatorTrigger.SetValueAndForceNotify("Refresh");
            Model.RefreshSheets();
        }
    }
}
