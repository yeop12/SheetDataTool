using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UniEx.UI;
using UniRx;
using Zenject;

namespace SheetDataTool
{
    public class PlatformGridItem : GridItemTemplate<PlatformInfo>
    {
	    [Inject] private SettingTab _settingTab;

	    private ReactiveProperty<bool> _showInvalidDefineNameObject;
	    private ReactiveProperty<bool> _showInvalidScriptPathObject;
	    private ReactiveProperty<bool> _showInvalidDataPathObject;

	    public List<TMP_Dropdown.OptionData> PlatformOptions { get; } = Enum.GetNames(typeof(Platform)).Select(x => new TMP_Dropdown.OptionData(x)).ToList();

		public int PlatformIndex
	    {
		    get => (int)Model.Platform;
		    set
		    {
			    Model.Platform = (Platform)value;
			    Model.NamespaceNames.Clear();

				switch (Model.Platform)
			    {
					case Platform.Unity:
						Model.NamespaceNames.Add("UnityEngine");
						Model.NamespaceNames.Add("UnityEngine.AddressableAssets");
						break;

					case Platform.CSharp:
						break;

					default:
						throw new NotImplementedException();
			    }
		    }

	    }

	    public string DefaultDirectory
	    {
			get => Model.DefaultDirectory;
			set => Model.DefaultDirectory = value;
	    }

	    public string DefineName
	    {
		    get => Model.DefineName;
		    set
		    {
			    Model.DefineName = value;
			    _showInvalidDefineNameObject.Value = string.IsNullOrWhiteSpace(value);
		    }
	    }

	    public string ScriptPath
	    {
		    get => Model.ScriptPath;
		    set
		    {
			    Model.ScriptPath = value;
			    _showInvalidScriptPathObject.Value = Directory.Exists(value) is false;
		    }
	    }

	    public string DataPath
	    {
		    get => Model.DataPath;
		    set
		    {
			    Model.DataPath = value;
			    _showInvalidDataPathObject.Value = Directory.Exists(value) is false;
			}
	    }

	    public IObservable<bool> ShowInvalidDefineNameObject => _showInvalidDefineNameObject;
	    public IObservable<bool> ShowInvalidScriptPathObject => _showInvalidScriptPathObject;
	    public IObservable<bool> ShowInvalidDataPathObject => _showInvalidDataPathObject;

	    protected override void OnInit(PlatformInfo model)
	    {
		    base.OnInit(model);
		    _showInvalidDefineNameObject = new ReactiveProperty<bool>();
		    _showInvalidScriptPathObject = new ReactiveProperty<bool>();
		    _showInvalidDataPathObject = new ReactiveProperty<bool>();
	    }

	    public void OnRemove()
	    {
			_settingTab.RemovePlatform(Model);
	    }
    }
}
