using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SheetDataTool.Model;
using UniEx.UI;
using UniRx;
using Zenject;

namespace SheetDataTool
{
    public class SheetGridItem : GridItemTemplate<string>
    {
	    [Inject] private Model.Tool _toolModel;
	    [Inject] private FixedUIManager _fixedUIManager;

	    private readonly ReactiveProperty<bool> _isWaitExporting = new();
	    private ReactiveProperty<string> _successAnimatorTrigger;

	    public string SheetName => Model;
		public IObservable<bool> IsWaitExporting => _isWaitExporting;
		public IObservable<bool> HasBookMark => _toolModel.WholeBookMarkSheetNames.ObserveCountChanged(true)
			.Select(x => _toolModel.WholeBookMarkSheetNames.Contains(Model));
		public IObservable<string> SuccessAnimatorTrigger => _successAnimatorTrigger;


		protected override void OnInit(string model)
		{
			_isWaitExporting.Value = false;
			_successAnimatorTrigger = new ReactiveProperty<string>();
		}

		public void OnExport()
		{
			_isWaitExporting.Value = true;
			UniTask.Void(async () =>
			{
				try
				{
					await Task.Factory.StartNew(() =>
					{
						var sheetInfo = _toolModel.SheetUtil.GetSheetInfo(Model);
						var contentsData = new ContentsData(sheetInfo, _toolModel.Setting.Value);
						var script = contentsData.GetScript(false);
						_toolModel.Setting.Value.PlatformInfos.ForEach(x =>
							File.WriteAllText($"{x.ScriptPath}/{Model}.cs", script));
						if (contentsData.HasDataFile)
						{
							var assembly = MakeAssembly(contentsData.GetScript(true),
								$"{_toolModel.Setting.Value.PlatformInfos.First().ScriptPath}\\{Model}.cs");
							var json = contentsData.Serialize(assembly);
							_toolModel.Setting.Value.PlatformInfos.ForEach(x =>
								File.WriteAllText($"{x.DataPath}/{Model}.json", json));
						}
					});
					_toolModel.Logs.Add(new Log($"{Model} sheet export success.", Model));
					_toolModel.LogCount.Value += 1;
					_successAnimatorTrigger.SetValueAndForceNotify("Success");
				}
				catch (Exception e)
				{
					_toolModel.Logs.Add(new Log(e, Model));
					_toolModel.LogCount.Value = 0;
					_fixedUIManager.Open<LogUI>(_toolModel.Logs);
				}

				_isWaitExporting.Value = false;
			});
		}
		
		private Assembly MakeAssembly( string script, string ignoreFileName ) 
		{
			var scripts = new List<string>();
			if (string.IsNullOrWhiteSpace(script) is false) {
				scripts.Add(script);
			}
			scripts.Add(ScriptUtil.GetUnityTypeScript());
			scripts.AddRange(Directory.GetFiles(_toolModel.Setting.Value.PlatformInfos.First().ScriptPath, "*.cs").Where(x => x != ignoreFileName).Select(File.ReadAllText));
			return CompileUtil.Compile("TempLib", scripts.ToArray());
		}

		public void OnOpenExcel()
	    {

	    }

	    public void OnToggleBookMark(bool isOn)
	    {
		    if (isOn) 
		    {
			    _toolModel.WholeBookMarkSheetNames.Add(SheetName);
			}
		    else
		    {
			    _toolModel.WholeBookMarkSheetNames.Remove(SheetName);
		    }
	    }
    }
}
