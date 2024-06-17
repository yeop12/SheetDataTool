using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Google;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SheetDataTool
{
	public class SheetDataToolWindow : EditorWindow
	{
		private const string AccessInfoKey = "SheetDataTool_AccessInfo";
		private const string SettingKey = "SheetDataTool_Setting";
		private const string SheetTypeKey = "SheetDataTool_SheetType";
		private const string ExcelFolderPathKey = "SheetDataTool_ExcelFolderPath";

		private enum Mode
		{
			AccessInfo,
			Setting,
			SheetList,
			Util,
		}

		[MenuItem("Window/SheetDataTool")]
		public static void ShowWindow() 
		{
			GetWindow(typeof(SheetDataToolWindow), false, "SheetDataTool");
		}

		private SheetType _sheetType;
		private string _excelFolderPath = string.Empty;
		private GoogleSheetAccessInfo _accessInfo;
		private Setting _setting;
		private Mode _mode;
		private SheetUtil _sheetUtil;
		private string _searchText;
		private Vector2 _scrollPosition;

		private void OnEnable()
		{
			if (PlayerPrefs.HasKey(AccessInfoKey))
			{
				var accessInfoJson = PlayerPrefs.GetString(AccessInfoKey);
				_accessInfo = JsonConvert.DeserializeObject<GoogleSheetAccessInfo>(accessInfoJson);
			}

			if (PlayerPrefs.HasKey(SettingKey))
			{
				var settingJson = PlayerPrefs.GetString(SettingKey);
				_setting = JsonConvert.DeserializeObject<Setting>(settingJson);
			}

			if (PlayerPrefs.HasKey(SheetTypeKey))
			{
				var sheetType = PlayerPrefs.GetInt(SheetTypeKey);
				_sheetType = (SheetType)sheetType;
			}

			if (PlayerPrefs.HasKey(ExcelFolderPathKey))
			{
				_excelFolderPath = PlayerPrefs.GetString(ExcelFolderPathKey);
			}

			if (_sheetType == SheetType.GoogleSheet && _accessInfo is null)
			{
				_accessInfo = new GoogleSheetAccessInfo();
				_mode = Mode.AccessInfo;
			}
			else if (_sheetType == SheetType.ExcelSheet && string.IsNullOrWhiteSpace(_excelFolderPath))
			{
				_mode = Mode.AccessInfo;
			}
			else if (MakeSheetUtil() is false)
			{
				_mode = Mode.AccessInfo;
			}
			else if (_setting is null)
			{
				_setting = new Setting();
				_mode = Mode.Setting;
			}
			else
			{
				_mode = Mode.SheetList;
			}
		}

		private bool MakeSheetUtil()
		{
			try
			{
				_sheetUtil = _sheetType switch
				{
					SheetType.GoogleSheet => new GoogleSheetUtil(_accessInfo.OAuthFilePath, _accessInfo.SheetID),
					SheetType.ExcelSheet => new ExcelSheetUtil(_excelFolderPath),
					_ => throw new NotImplementedException($"{_sheetType}")
				};
				return true;
			}
			catch (FileNotFoundException e)
			{
				EditorUtility.DisplayDialog("Error", "OAuth file path is invalid.", "Ok");
				Debug.LogError(e);
			}
			catch (InvalidOperationException e)
			{
				EditorUtility.DisplayDialog("Error", "OAuth file is invalid.", "Ok");
				Debug.LogError(e);
			}
			catch (GoogleApiException e)
			{
				EditorUtility.DisplayDialog("Error", "Spread sheet id is invalid.", "Ok");
				Debug.LogError(e);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			return false;
		}

		private void OnGUI()
		{
			switch (_mode)
			{
				case Mode.AccessInfo:
					DrawAccessInfo();
					break;

				case Mode.Setting:
					DrawSetting();
					break;

				case Mode.SheetList:
					DrawSheetList();
					break;

				case Mode.Util:
					DrawUtil();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void DrawAccessInfo()
		{
			EditorGUILayout.LabelField("Access Information", new GUIStyle(){fontSize = 20, normal = new GUIStyleState(){textColor = Color.white}});
			EditorGUILayout.Separator();

			_sheetType = (SheetType)EditorGUILayout.EnumPopup("Sheet type", _sheetType);

			switch (_sheetType)
			{
				case SheetType.GoogleSheet:
					EditorGUILayout.LabelField("OAuth file path");
					EditorGUILayout.BeginHorizontal();
					_accessInfo.OAuthFilePath = EditorGUILayout.TextField(_accessInfo.OAuthFilePath);
					if (GUILayout.Button("Find", GUILayout.MaxWidth(70)))
					{
						var path = EditorUtility.OpenFilePanel("OAuth file", Application.dataPath, "json");
						if (path.Length != 0)
						{
							_accessInfo.OAuthFilePath = path;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.LabelField("Spread sheet id");
					_accessInfo.SheetID = EditorGUILayout.TextField(_accessInfo.SheetID);
					break;

				case SheetType.ExcelSheet:
					EditorGUILayout.BeginHorizontal();
					_excelFolderPath = EditorGUILayout.TextField(_excelFolderPath);
					if (GUILayout.Button("Find", GUILayout.MaxWidth(70)))
					{
						var path = EditorUtility.OpenFolderPanel("Excel folder", Application.dataPath, "");
						if (path.Length != 0)
						{
							_excelFolderPath = path;
						}
					}
					EditorGUILayout.EndHorizontal();
					break;

				default:
					throw new NotImplementedException($"{_sheetType}");
			}
			EditorGUILayout.Separator();

			if (GUILayout.Button("Save"))
			{
				if (MakeSheetUtil())
				{
					PlayerPrefs.SetInt(SheetTypeKey, (int)_sheetType);
					switch (_sheetType)
					{
						case SheetType.GoogleSheet:
							var accessInfoJson = JsonConvert.SerializeObject(_accessInfo);
							PlayerPrefs.SetString(AccessInfoKey, accessInfoJson);
							break;

						case SheetType.ExcelSheet:
							PlayerPrefs.SetString(ExcelFolderPathKey, _excelFolderPath);
							break;

						default:
							throw new NotImplementedException($"{_sheetType}");
					}
					PlayerPrefs.Save();
					if (_setting is null)
					{
						_mode = Mode.Setting;
						_setting = new Setting();
					}
					else
					{
						_mode = Mode.SheetList;
					}
				}
			}

			if (PlayerPrefs.HasKey(AccessInfoKey) && GUILayout.Button("Back"))
			{
				_mode = Mode.SheetList;
			}
		}

		private void DrawSetting()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			EditorGUILayout.LabelField("Setting", new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.white } });
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Google Sheet");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.InputNotation = (Notation)EditorGUILayout.EnumPopup("Input notation", _setting.InputNotation);
			_setting.IgnoreLineSymbol = EditorGUILayout.TextField("Ignore line symbol", new string(_setting.IgnoreLineSymbol, 1))[0];
			_setting.EnumDefaultType = EditorGUILayout.TextField("Enum default type", _setting.EnumDefaultType);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Export Script");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");

			EditorGUILayout.LabelField("Class");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ScriptClassNameNotation = (Notation)EditorGUILayout.EnumPopup("Name notation", _setting.ScriptClassNameNotation);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Enum");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ScriptEnumNameNotation = (Notation)EditorGUILayout.EnumPopup("Name notation", _setting.ScriptEnumNameNotation);
			_setting.ScriptEnumItemNameNotation = (Notation)EditorGUILayout.EnumPopup("Item name notation", _setting.ScriptEnumItemNameNotation);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Record");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ScriptRecordNameNotation = (Notation)EditorGUILayout.EnumPopup("Name notation", _setting.ScriptRecordNameNotation);
			_setting.ScriptRecordPropertyNameNotation = (Notation)EditorGUILayout.EnumPopup("Property name notation", _setting.ScriptRecordPropertyNameNotation);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Design");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ScriptPublicVariableNameNotation = (Notation)EditorGUILayout.EnumPopup("Public variable name notation", _setting.ScriptPublicVariableNameNotation);
			_setting.ScriptPrivateVariableNameNotation = (Notation)EditorGUILayout.EnumPopup("Private variable name notation", _setting.ScriptPrivateVariableNameNotation);
			_setting.ScriptPrivateVariableNamePrefix = EditorGUILayout.TextField("Private variable name prefix", _setting.ScriptPrivateVariableNamePrefix);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Interface");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ScriptInterfaceNameNotation = (Notation)EditorGUILayout.EnumPopup("Name notation", _setting.ScriptInterfaceNameNotation);
			_setting.ScriptInterfaceNamePrefix = EditorGUILayout.TextField("Name prefix", _setting.ScriptInterfaceNamePrefix);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Function");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ScriptFunctionNameNotation = (Notation)EditorGUILayout.EnumPopup("Function name notation", _setting.ScriptFunctionNameNotation);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("ETC");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.NamespaceName = EditorGUILayout.TextField("Namespace name", _setting.NamespaceName);
			EditorGUILayout.LabelField("Platforms");
			foreach (var platformInfo in _setting.PlatformInfos)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.BeginVertical("Box");
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Platform");
				if (GUILayout.Button("X", GUILayout.MaxWidth(30)))
				{
					_setting.PlatformInfos.Remove(platformInfo);
					GUIUtility.ExitGUI();
					return;
				}

				EditorGUILayout.EndHorizontal();
				platformInfo.Platform = (Platform)EditorGUILayout.EnumPopup("Platform", platformInfo.Platform);
				platformInfo.DefaultDirectory = EditorGUILayout.TextField("Default directory", platformInfo.DefaultDirectory);
				platformInfo.DefineName = EditorGUILayout.TextField("Define name", platformInfo.DefineName);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Script path");
				platformInfo.ScriptPath = EditorGUILayout.TextField(platformInfo.ScriptPath);
				if (GUILayout.Button("Find", GUILayout.MaxWidth(70))) 
				{
					var path = EditorUtility.OpenFolderPanel("Scrip path", Application.dataPath, "");
					if (path.Length != 0) 
					{
						platformInfo.ScriptPath = path;
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Data path");
				platformInfo.DataPath = EditorGUILayout.TextField(platformInfo.DataPath);
				if (GUILayout.Button("Find", GUILayout.MaxWidth(70))) 
				{
					var path = EditorUtility.OpenFolderPanel("Data path", Application.dataPath, "");
					if (path.Length != 0) 
					{
						platformInfo.DataPath = path;
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.LabelField("Namespace names");
				++EditorGUI.indentLevel;
				EditorGUILayout.BeginVertical("Box");
				for (var i = 0; i < platformInfo.NamespaceNames.Count; ++i)
				{
					EditorGUILayout.BeginHorizontal();
					platformInfo.NamespaceNames[i] = EditorGUILayout.TextField(platformInfo.NamespaceNames[i]);
					if (GUILayout.Button("X", GUILayout.MaxWidth(30)))
					{
						platformInfo.NamespaceNames.RemoveAt(i);
						GUIUtility.ExitGUI();
						return;
					}
					EditorGUILayout.EndHorizontal();
				}

				if (GUILayout.Button("Add namespace name"))
				{
					platformInfo.NamespaceNames.Add(string.Empty);
				}
				EditorGUILayout.EndVertical();
				--EditorGUI.indentLevel;

				EditorGUILayout.EndVertical();
				--EditorGUI.indentLevel;

				EditorGUILayout.Separator();
			}
			if (GUILayout.Button("Add Platform"))
			{
				_setting.PlatformInfos.Add(new PlatformInfo());
			}
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;

			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			if (GUILayout.Button("Save"))
			{
				var settingJson = JsonConvert.SerializeObject(_setting);
				PlayerPrefs.SetString(SettingKey, settingJson);
				PlayerPrefs.Save();
				_mode = Mode.SheetList;
			}

			if (PlayerPrefs.HasKey(SettingKey) && GUILayout.Button("Back"))
			{
				_mode = Mode.SheetList;
			}
			EditorGUILayout.EndScrollView();
		}

		private void DrawSheetList()
		{
			EditorGUILayout.LabelField("Sheet List", new GUIStyle(){fontSize = 20, normal = new GUIStyleState(){textColor = Color.white}});
			EditorGUILayout.Separator();

			_searchText = GUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
			EditorGUILayout.Separator();
			var useSearch = string.IsNullOrWhiteSpace(_searchText) is false;
			var lowerSearchText = _searchText?.ToLower();

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			foreach (var sheetName in _sheetUtil.GetSheetNames())
			{
				if (useSearch && sheetName.ToLower().Contains(lowerSearchText!) is false)
				{
					continue;
				}
				EditorGUILayout.BeginHorizontal("Box");
				EditorGUILayout.LabelField(sheetName);
				if (GUILayout.Button("CS", GUILayout.Width(30)))
				{
					try
					{
						var sheetInfo = _sheetUtil.GetSheetInfo(sheetName);
						var contentsData = new ContentsData(sheetInfo, _setting);
						var script = contentsData.GetScript(false);
						_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{sheetName}.cs", script));
						if (contentsData.HasDataFile)
						{
							var assembly = MakeAssembly(contentsData.GetScript(true), $"{_setting.PlatformInfos.First().ScriptPath}\\{sheetName}.cs");
							var json = contentsData.Serialize(assembly);
							_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.DataPath}/{sheetName}.json", json));
						}
						AssetDatabase.Refresh(ImportAssetOptions.Default);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}

				if (GUILayout.Button("ED", GUILayout.Width(30)))
				{
					try
					{
						var sheetInfo = _sheetUtil.GetSheetInfo(sheetName);
						var contentsData = new ContentsData(sheetInfo, _setting);
						if (contentsData.HasDataFile)
						{
							var assembly = MakeAssembly(contentsData.GetScript(true), $"{_setting.PlatformInfos.First().ScriptPath}\\{sheetName}.cs");
							var json = contentsData.Serialize(assembly);
							_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.DataPath}/{sheetName}.json", json));
						}
						AssetDatabase.Refresh(ImportAssetOptions.Default);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			if (GUILayout.Button("AccessInfo"))
			{
				_mode = Mode.AccessInfo;
			}

			if (GUILayout.Button("Setting"))
			{
				_mode = Mode.Setting;
			}

			if (GUILayout.Button("Util"))
			{
				_mode = Mode.Util;
			}

			if (GUILayout.Button("Refresh"))
			{
				_sheetUtil.RefreshSheetList();
			}
		}

		private Assembly MakeAssembly(string script, string ignoreFileName)
		{
			var scripts = new List<string>();
			if (string.IsNullOrWhiteSpace(script) is false)
			{
				scripts.Add(script);
			}
			scripts.Add(ScriptUtil.GetUnityTypeScript());
			scripts.AddRange(Directory.GetFiles(_setting.PlatformInfos.First().ScriptPath, "*.cs").Where(x => x != ignoreFileName).Select(File.ReadAllText));
			return CompileUtil.Compile("TempLib", scripts.ToArray());
		}

		private void DrawUtil() 
		{
			EditorGUILayout.LabelField("Util", new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.white } });
			EditorGUILayout.Separator();
			
			if (GUILayout.Button("All"))
			{
				var script = ScriptUtil.GetBaseClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetBaseClassName(_setting)}.cs", script));
				script = ScriptUtil.GetDesignInterfaceScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetDesignInterfaceName(_setting)}.cs", script));
				script = ScriptUtil.GetDesignClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetDesignClassName(_setting)}.cs", script));
				script = ScriptUtil.GetConstantClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetConstantClassName(_setting)}.cs", script));
				script = ScriptUtil.GetFullClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetFullClassName(_setting)}.cs", script));
				script = ScriptUtil.GetExcelDataNotFoundExceptionScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetExcelDataNotFoundExceptionName(_setting)}.cs", script));
				script = ScriptUtil.GetExternalInitScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetExternalInitName()}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Base class"))
			{
				var script = ScriptUtil.GetBaseClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetBaseClassName(_setting)}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Design interface")) 
			{
				var script = ScriptUtil.GetDesignInterfaceScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetDesignInterfaceName(_setting)}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Design class"))
			{
				var script = ScriptUtil.GetDesignClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetDesignClassName(_setting)}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Constant class"))
			{
				var script = ScriptUtil.GetConstantClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetConstantClassName(_setting)}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Full class"))
			{
				var script = ScriptUtil.GetFullClassScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetFullClassName(_setting)}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("ExcelDataNotFoundException class"))
			{
				var script = ScriptUtil.GetExcelDataNotFoundExceptionScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetExcelDataNotFoundExceptionName(_setting)}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("External init class"))
			{
				var script = ScriptUtil.GetExternalInitScript(_setting);
				_setting.PlatformInfos.ForEach(x => File.WriteAllText($"{x.ScriptPath}/{ScriptUtil.GetExternalInitName()}.cs", script));
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Back"))
			{
				_mode = Mode.SheetList;
			}
		}
	}
}
