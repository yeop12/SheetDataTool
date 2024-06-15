using System;
using System.IO;
using UniEx.UI;
using Zenject;

namespace SheetDataTool
{
    public class UtilTab : ControlItemTemplate<Setting>
    {
	    [Inject] private FixedUIManager _fixedUIManager;

	    public void OnCreateAll()
	    {
		    OnCreateBaseClass(false);
		    OnCreateDesignInterface(false);
		    OnCreateDesignClass(false);
		    OnCreateConstantClass(false);
		    OnCreateFullClass(false);
		    OnCreateExcelNotFoundExceptionClass(false);
		    OnCreateExternalInitClass(false);
		    _fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
	    }

	    public void OnCreateBaseClass( bool openMessagePopupUI )
	    {
		    var script = ScriptUtil.GetBaseClassScript(Model);
		    Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetBaseClassName(Model)}.cs", script));
		    if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}

	    public void OnCreateDesignInterface( bool openMessagePopupUI )
	    {
		    var script = ScriptUtil.GetDesignInterfaceScript(Model);
		    Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetDesignInterfaceName(Model)}.cs", script));
		    if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}

	    public void OnCreateDesignClass( bool openMessagePopupUI ) 
	    {
			var script = ScriptUtil.GetDesignClassScript(Model);
			Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetDesignClassName(Model)}.cs", script));
			if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}

	    public void OnCreateConstantClass( bool openMessagePopupUI )
	    {
		    var script = ScriptUtil.GetConstantClassScript(Model);
		    Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetConstantClassName(Model)}.cs", script));
		    if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}

	    public void OnCreateFullClass( bool openMessagePopupUI )
	    {
		    var script = ScriptUtil.GetFullClassScript(Model);
		    Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetFullClassName(Model)}.cs", script));
		    if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}

	    public void OnCreateExcelNotFoundExceptionClass( bool openMessagePopupUI )
	    {
		    var script = ScriptUtil.GetExcelDataNotFoundExceptionScript(Model);
		    Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetExcelDataNotFoundExceptionName(Model)}.cs", script));
		    if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}

	    public void OnCreateExternalInitClass( bool openMessagePopupUI )
	    {
		    var script = ScriptUtil.GetExternalInitScript(Model);
		    Model.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetExternalInitName()}.cs", script));
		    if (openMessagePopupUI)
		    {
				_fixedUIManager.Open<MessagePopupUI>(("Create success.", (Action)null));
		    }
		}
    }
}
