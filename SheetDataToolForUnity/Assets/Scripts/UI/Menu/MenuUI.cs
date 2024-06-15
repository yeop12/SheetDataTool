using System;
using UniEx.UI;
using UniRx;
using Zenject;

namespace SheetDataTool
{
    [AddressableInfo("UI/Menu")]
    public class MenuUI : FixedUIWindowTemplate<Model.Tool>
    {
	    [Inject] private FixedUIManager _fixedUIManager;

	    private ReactiveProperty<Tuple<AccessInfo, Action>> _accessInfoTab;
	    private ReactiveProperty<Tuple<Setting, Action>> _settingTab;
	    private ReactiveProperty<Setting> _utilTab;

		public IObservable<bool> HasAccessInfoAndSetting =>
		    Model.AccessInfo.CombineLatest(Model.Setting, (accessInfo, setting) => accessInfo is not null && setting is not null);
	    public IObservable<bool> HasAccessInfo => Model.AccessInfo.Select(x => x is not null);
	    public IObservable<Tuple<AccessInfo, Action>> AccessInfoTab => _accessInfoTab;
		public IObservable<Tuple<Setting, Action>> SettingTab => _settingTab;
		public IObservable<Setting> UtilTab => _utilTab;

	    protected override void OnOpen(Model.Tool model)
	    {
		    _accessInfoTab = new ReactiveProperty<Tuple<AccessInfo, Action>>(model.AccessInfo.Value is null
			    ? new Tuple<AccessInfo, Action>(new AccessInfo(), OnShowSetting)
			    : null);
		    _settingTab = new ReactiveProperty<Tuple<Setting, Action>>(model.AccessInfo.Value is null
			    ? null
			    : new Tuple<Setting, Action>(model.Setting.Value ?? new Setting(),
				    model.Setting.Value is null ? OnShowUtil : null));
		    _utilTab = new ReactiveProperty<Setting>();
	    }

	    public void OnShowInfo()
	    {
		    if (_accessInfoTab.Value is not null)
		    {
			    return;
		    }

		    _accessInfoTab.Value = new Tuple<AccessInfo, Action>(Model.AccessInfo.Value ?? new AccessInfo(), null);
		    _settingTab.Value = null;
		    _utilTab.Value = null;
	    }

	    public void OnShowSetting()
	    {
		    if (_settingTab.Value is not null)
		    {
			    return;
		    }

		    _accessInfoTab.Value = null;
		    _settingTab.Value = new Tuple<Setting, Action>(Model.Setting.Value ?? new Setting(), Model.Setting.Value is null ? OnShowUtil : null);
		    _utilTab.Value = null;
		}

	    public void OnShowUtil()
	    {
		    if (_utilTab.Value is not null)
		    {
			    return;
		    }

		    _accessInfoTab.Value = null;
		    _settingTab.Value = null;
		    _utilTab.Value = Model.Setting.Value;
		}

	    public void OnClose()
	    {
		    Close();
		    if (_fixedUIManager.IsOpened<MainUI>() is false)
		    {
			    _fixedUIManager.Open<MainUI>(Model);
		    }
	    }
    }
}
