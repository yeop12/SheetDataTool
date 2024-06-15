using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UniRx;
using Unity.VisualScripting;

namespace SheetDataTool.Model
{ 
    public class Tool
    {
	    public const string SharedDataFilePath = "./SharedData.txt";
	    public const string NonSharedFilePath = "./NonSharedData.txt";
	    public const string LogFilePath = "./Log.txt";

		private readonly ReactiveCollection<string> _visibleNormalSheetNames;
	    private readonly ReactiveCollection<string> _visibleBookMarkSheetNames;
		private readonly ReactiveProperty<AccessInfo> _accessInfo = new();

		public IReadOnlyReactiveProperty<AccessInfo> AccessInfo => _accessInfo;
		public ReactiveProperty<Setting> Setting { get; }
	    public ReactiveCollection<string> WholeNormalSheetNames { get; } = new();
	    public ReactiveCollection<string> WholeBookMarkSheetNames { get; }
	    public IReadOnlyReactiveCollection<string> VisibleNormalSheetNames => _visibleNormalSheetNames;
	    public IReadOnlyReactiveCollection<string> VisibleBookMarkSheetNames => _visibleBookMarkSheetNames;
		public ReactiveProperty<int> LogCount { get; } = new();
	    public ReactiveProperty<string> SearchText { get; } = new(string.Empty);
		public SheetUtil SheetUtil { get; private set; }

		public ReactiveCollection<Log> Logs = new();

		public Tool(AccessInfo accessInfo, Setting setting, List<string> bookMarkSheetNames)
	    {
			SetAccessInfo(accessInfo, false);
		    Setting = new ReactiveProperty<Setting>(setting);
		    WholeBookMarkSheetNames = new ReactiveCollection<string>(bookMarkSheetNames);
		    _visibleNormalSheetNames = new ReactiveCollection<string>(WholeNormalSheetNames);
		    _visibleBookMarkSheetNames = new ReactiveCollection<string>(bookMarkSheetNames);
		    SearchText.Subscribe(x =>
		    {
			    _visibleNormalSheetNames.Clear();
			    _visibleNormalSheetNames.AddRange(WholeNormalSheetNames.Where(sheetName =>
				    string.IsNullOrWhiteSpace(x) || sheetName.Contains(x)));
			    _visibleBookMarkSheetNames.Clear();
			    _visibleBookMarkSheetNames.AddRange(WholeBookMarkSheetNames.Where(sheetName =>
				    string.IsNullOrWhiteSpace(x) || sheetName.Contains(x)));
			});
			WholeNormalSheetNames.ObserveCountChanged().Subscribe(x =>
		    {
			    _visibleNormalSheetNames.Clear();
			    _visibleNormalSheetNames.AddRange(WholeNormalSheetNames.Where(sheetName =>
				    string.IsNullOrWhiteSpace(SearchText.Value) || sheetName.Contains(SearchText.Value)).OrderBy(sheetName => sheetName));
		    });
		    WholeBookMarkSheetNames.ObserveCountChanged().Subscribe(x =>
		    {
			    _visibleBookMarkSheetNames.Clear();
			    _visibleBookMarkSheetNames.AddRange(WholeBookMarkSheetNames.Where(sheetName =>
				    string.IsNullOrWhiteSpace(SearchText.Value) || sheetName.Contains(SearchText.Value)).OrderBy(sheetName => sheetName));
			    var data = JsonConvert.SerializeObject(WholeBookMarkSheetNames.ToList());
			    File.WriteAllText(NonSharedFilePath, data);
			});
		    Setting.Subscribe(x => 
		    {
			    var data = JsonConvert.SerializeObject((AccessInfo.Value, x));
				File.WriteAllText(SharedDataFilePath, data);
			});
	    }

		public void SetAccessInfo(AccessInfo accessInfo, bool saveFile = true)
		{
		    SheetUtil = null;
		    WholeNormalSheetNames.Clear();
		    if (accessInfo is not null)
		    {
			    try
			    {
				    SheetUtil = accessInfo.GetSheetUtil();
				    WholeNormalSheetNames.AddRange(SheetUtil.GetSheetNames().ToList().OrderBy(x => x));
				    if (saveFile)
				    {
					    var data = JsonConvert.SerializeObject((accessInfo, Setting.Value));
						File.WriteAllText(SharedDataFilePath, data);
				    }
			    }
			    catch
			    {
				    accessInfo = null;
			    }
		    }

		    _accessInfo.Value = accessInfo;
		}

		public void RefreshSheets()
		{
			if (SheetUtil is null)
			{
				return;
			}

			SheetUtil.RefreshSheetList();
			WholeNormalSheetNames.Clear();
			WholeNormalSheetNames.AddRange(SheetUtil.GetSheetNames().ToList().OrderBy(x => x));
		}
	}
} 
