using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniEx;
using UniEx.UI;
using UniRx;
using Zenject;

namespace SheetDataTool
{
	public class SettingTab : ControlItemTemplate<Tuple<Setting, Action>>
	{
		[Inject] private FixedUIManager _fixedUIManager;
		[Inject] private Model.Tool _toolModel;
		[Inject] private DiContainer _container;

		private Setting _setting;
		private Action _onSave;
		private ReactiveCollection<PlatformInfo> _platforms;

		public List<TMP_Dropdown.OptionData> NotationOptions { get; } = Enum.GetNames(typeof(Notation)).Select(x => new TMP_Dropdown.OptionData(x)).ToList();

		public int InputNotationIndex 
		{
			get => (int) _setting.InputNotation;
			set => _setting.InputNotation = (Notation)value;
		}

		public string IgnoreLineSymbol
		{
            get => $"{_setting.IgnoreLineSymbol}";
            set => _setting.IgnoreLineSymbol = value;
		}

		public string EnumDefaultType
		{
			get => _setting.EnumDefaultType;
            set => _setting.EnumDefaultType = value;
		}

		public int EnumNameNotationIndex
		{
			get => (int)_setting.ScriptEnumNameNotation;
			set => _setting.ScriptEnumNameNotation = (Notation)value;
		}

		public int EnumItemNameNotationIndex
		{
			get => (int)_setting.ScriptEnumItemNameNotation;
			set => _setting.ScriptEnumItemNameNotation = (Notation)value;
		}

		public int RecordNameNotationIndex
		{
			get => (int)_setting.ScriptRecordNameNotation;
			set => _setting.ScriptRecordNameNotation = (Notation)value;
		}

		public int PublicVariableNameNotationIndex
		{
			get => (int)_setting.ScriptPublicVariableNameNotation;
			set => _setting.ScriptPublicVariableNameNotation = (Notation)value;	
		}

		public int PrivateVariableNameNotationIndex
		{
			get => (int)_setting.ScriptPrivateVariableNameNotation;
			set => _setting.ScriptPrivateVariableNameNotation = (Notation)value;
		}

		public string PrivateVariableNamePrefix
		{
			get => _setting.ScriptPrivateVariableNamePrefix;
			set => _setting.ScriptPrivateVariableNamePrefix = value;
		}

		public string ReferenceReplacementSymbol
		{
			get => _setting.ReferenceReplacementSymbol;
			set => _setting.ReferenceReplacementSymbol = value;
		}

		public string ReferenceReplacementWord
		{
			get => _setting.ReferenceReplacementWord;
			set => _setting.ReferenceReplacementWord = value;
		}

		public int InterfaceNameNotationIndex
		{
			get => (int)_setting.ScriptInterfaceNameNotation;
			set => _setting.ScriptInterfaceNameNotation = (Notation)value;
		}

		public string InterfaceNamePrefix
		{
			get => _setting.ScriptInterfaceNamePrefix;
			set => _setting.ScriptInterfaceNamePrefix = value;
		}

		public int FunctionNameNotationIndex
		{
			get => (int)_setting.ScriptFunctionNameNotation;
			set => _setting.ScriptFunctionNameNotation = (Notation)value;
		}

		public string NamespaceName
		{
			get => _setting.NamespaceName;
			set => _setting.NamespaceName = value;
		}

		public bool UseContentsData
		{
			get => _setting.UseDataContents;
			set => _setting.UseDataContents = value;
		}

		public ReadOnlyReactiveCollectionWrapper<PlatformInfo> Platforms => new(_platforms);

        protected override void OnInit( Tuple<Setting, Action> settingAndOnSave )
        {
	        _setting = settingAndOnSave.Item1 with { };
	        _onSave = settingAndOnSave.Item2;
			_platforms = new ReactiveCollection<PlatformInfo>(_setting.PlatformInfos);
	        _container.Bind<SettingTab>().FromInstance(this).NonLazy();
        }

        private void OnDisable()
        {
	        _container.Unbind<SettingTab>();
        }

        public void OnSave()
        {
	        if (_platforms.All(x => x.IsValid()) is false)
	        {
				_fixedUIManager.Open<MessagePopupUI>(("Save failed.", (Action)null));
				return;
	        }

	        _toolModel.Setting.Value = _setting with { PlatformInfos = _platforms.ToList() };
	        _fixedUIManager.Open<MessagePopupUI>(("Save success.", _onSave));
		}

        public void OnAddPlatform()
        {
	        _platforms.Add(new PlatformInfo()
	        {
		        NamespaceNames = new List<string>() { "UnityEngine", "UnityEngine.AddressableAssets" },
				DefineName = "UNITY_2020_1_OR_NEWER",
			});
        }

        public void RemovePlatform(PlatformInfo platformInfo)
        {
	        _platforms.Remove(platformInfo);
        }
    }
}
