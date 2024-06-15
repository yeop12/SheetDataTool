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
    public class AccessInfoTab : ControlItemTemplate<Tuple<AccessInfo, Action>>
    {
		[Inject] private FixedUIManager _fixedUIManager;
		[Inject] private Model.Tool _toolModel;

	    private AccessInfo _accessInfo;
	    private Action _onSave;
	    private ReactiveProperty<bool> _showGoogleSheetId;
	    private ReactiveProperty<bool> _showInvalidPathObject;

	    public List<TMP_Dropdown.OptionData> SheetTypeOptions =>
		    Enum.GetNames(typeof(SheetType)).Select(x => new TMP_Dropdown.OptionData(x)).ToList();

	    public int SheetTypeIndex
	    {
		    get => (int)_accessInfo.SheetType;
		    set 
		    {
				_accessInfo.SheetType = (SheetType)value;
				_showGoogleSheetId.Value = _accessInfo.SheetType == SheetType.GoogleSheet;
			}
	    }

	    public string Path
	    {
			get => _accessInfo.Path;
			set
			{
				_accessInfo.Path = value;
				if (_accessInfo.SheetType == SheetType.ExcelSheet)
				{
					_showInvalidPathObject.Value = Directory.Exists(value) is false;
				}
			}

	    }

	    public string GoogleSheetId
	    {
			get => _accessInfo.GoogleSheetId;
			set => _accessInfo.GoogleSheetId = value;
	    }

		public IObservable<bool> ShowGoogleSheetId => _showGoogleSheetId;
		public IObservable<bool> ShowInvalidPathObject => _showInvalidPathObject;

		protected override void OnInit(Tuple<AccessInfo, Action> accessInfoAndOnSave )
	    {
		    _accessInfo = accessInfoAndOnSave.Item1 with { };
		    _onSave = accessInfoAndOnSave.Item2;
			_showGoogleSheetId = new ReactiveProperty<bool>(_accessInfo.SheetType == SheetType.GoogleSheet);
			_showInvalidPathObject = new ReactiveProperty<bool>();
		}

	    public void OnSave()
	    {
		    if (_accessInfo.IsValid() is false)
		    {
			    _fixedUIManager.Open<MessagePopupUI>(("Save failed.", (Action)null));
				return;
		    }

		    _toolModel.SetAccessInfo(_accessInfo);
			var message = _toolModel.AccessInfo.Value is null ? "Save failed." : "Save success.";
			_fixedUIManager.Open<MessagePopupUI>((message, _toolModel.AccessInfo.Value is null ? null : _onSave));
		}
    }
}
