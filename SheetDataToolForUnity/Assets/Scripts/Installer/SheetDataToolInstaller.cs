using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UniEx.UI;
using UnityEngine;
using Zenject;

namespace SheetDataTool
{
    public class SheetDataToolInstaller : MonoInstaller
    {
	    [SerializeField] private FixedUIManager _fixedUIManager;

	    private Model.Tool _toolModel;

	    public override void InstallBindings()
	    {
		    Container.Bind<FixedUIManager>().FromInstance(_fixedUIManager).AsSingle().NonLazy();
	    }

	    public override void Start()
	    {
			base.Start();

			InitSheetDataToolModel();
			if (_toolModel.AccessInfo.Value is null || _toolModel.Setting.Value is null)
			{
				_fixedUIManager.Open<MenuUI>(_toolModel);
			}
			else
			{
				_fixedUIManager.Open<MainUI>(_toolModel);
			}
	    }

	    private void InitSheetDataToolModel()
	    {
		    AccessInfo accessInfo = null;
		    Setting setting = null;
		    if (File.Exists(Model.Tool.SharedDataFilePath))
		    {
			    var data = File.ReadAllText(Model.Tool.SharedDataFilePath);
			    try
			    {
				    (accessInfo, setting) = JsonConvert.DeserializeObject<(AccessInfo, Setting)>(data);
			    }
			    catch
			    {
				    // ignored
			    }
		    }
			
			var bookMarkSheetNames = new List<string>();
		    if (File.Exists(Model.Tool.NonSharedFilePath))
		    {
			    var data = File.ReadAllText(Model.Tool.NonSharedFilePath);
				bookMarkSheetNames.AddRange(JsonConvert.DeserializeObject<List<string>>(data)!);
			}

		    _toolModel = new Model.Tool(accessInfo, setting, bookMarkSheetNames);
		    Container.Bind<Model.Tool>().FromInstance(_toolModel);
	    }
    }
}
