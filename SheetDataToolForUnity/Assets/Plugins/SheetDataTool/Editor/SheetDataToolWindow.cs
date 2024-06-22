using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Google;
using UnityEditor;
using UnityEngine;

namespace SheetDataTool
{
	public class SheetDataToolWindow : EditorWindow
	{
		private enum Mode
		{
			Information,
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

		private Information _information;
		private AccessInfo _accessInfo;
		private Setting _setting;
		private Mode _mode;
		private SheetUtil _sheetUtil;
		private string _searchText;
		private Vector2 _scrollPosition;

		private void OnEnable()
		{
			var informationGUIDs = AssetDatabase.FindAssets("t:SheetDataTool.Information");
			if (informationGUIDs.Any())
			{
				foreach (var informationGUID in informationGUIDs)
				{
					var path = AssetDatabase.GUIDToAssetPath(informationGUID);
					try
					{
						_information = AssetDatabase.LoadAssetAtPath<Information>(path);
						break;
					}
					catch
					{
						// ignore
					}
				}
			}

			if (_information is null)
			{
				_mode = Mode.Information;
				return;
			}

			_accessInfo = _information.AccessInfo;
			_setting = _information.Setting;
			
			if (_accessInfo is null || MakeSheetUtil() is false)
			{
				_accessInfo = new AccessInfo();
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
				_sheetUtil = _accessInfo.SheetType switch
				{
					SheetType.GoogleSheet => new GoogleSheetUtil(_accessInfo.Path, _accessInfo.GoogleSheetId),
					SheetType.ExcelSheet => new ExcelSheetUtil(_accessInfo.Path),
					_ => throw new NotImplementedException($"{_accessInfo.SheetType}")
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
				case Mode.Information:
					DrawInformation();
					break;

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

		private void DrawInformation()
		{
			if (GUILayout.Button("Create Information"))
			{
				var path = EditorUtility.SaveFilePanelInProject("Create information", "Information", "asset", string.Empty);
				if (path.Length != 0)
				{
					_information = CreateInstance<Information>();
					AssetDatabase.CreateAsset(_information, path);
					AssetDatabase.SaveAssets();

					_accessInfo = new AccessInfo();
					_mode = Mode.AccessInfo;
				}
			}
		}

		private void DrawAccessInfo()
		{
			EditorGUILayout.LabelField("Access Information", new GUIStyle(){fontSize = 20, normal = new GUIStyleState(){textColor = Color.white}});
			EditorGUILayout.Separator();

			_accessInfo.SheetType = (SheetType)EditorGUILayout.EnumPopup("Sheet type", _accessInfo.SheetType);

			switch (_accessInfo.SheetType)
			{
				case SheetType.GoogleSheet:
					EditorGUILayout.LabelField("OAuth file path");
					EditorGUILayout.BeginHorizontal();
					_accessInfo.Path = EditorGUILayout.TextField(_accessInfo.Path);
					if (GUILayout.Button("Find", GUILayout.MaxWidth(70)))
					{
						var path = EditorUtility.OpenFilePanel("OAuth file", Application.dataPath, "json");
						if (path.Length != 0)
						{
							_accessInfo.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.LabelField("Spread sheet id");
					_accessInfo.GoogleSheetId = EditorGUILayout.TextField(_accessInfo.GoogleSheetId);
					break;

				case SheetType.ExcelSheet:
					EditorGUILayout.BeginHorizontal();
					_accessInfo.Path = EditorGUILayout.TextField(_accessInfo.Path);
					if (GUILayout.Button("Find", GUILayout.MaxWidth(70)))
					{
						var path = EditorUtility.OpenFolderPanel("Excel folder", Application.dataPath, "");
						if (path.Length != 0)
						{
							_accessInfo.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), path); ;
						}
					}
					EditorGUILayout.EndHorizontal();
					break;

				default:
					throw new NotImplementedException($"{_accessInfo.SheetType}");
			}
			EditorGUILayout.Separator();

			if (GUILayout.Button("Save"))
			{
				if (MakeSheetUtil())
				{
					_information.AccessInfo = _accessInfo;
					EditorUtility.SetDirty(_information);
					AssetDatabase.SaveAssets();
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

			if (_information.IsValid() && GUILayout.Button("Back"))
			{
				_mode = Mode.SheetList;
			}
		}

		private void DrawSetting()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			EditorGUILayout.LabelField("Setting", new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.white } });
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Import Sheet");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.InputNotation = (Notation)EditorGUILayout.EnumPopup("Input notation", _setting.InputNotation);
			_setting.IgnoreLineSymbol = EditorGUILayout.TextField("Ignore line symbol", _setting.IgnoreLineSymbol);
			_setting.EnumDefaultType = EditorGUILayout.TextField("Enum default type", _setting.EnumDefaultType);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Export Script");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");

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
			_setting.ScriptPublicVariableNameNotation = (Notation)EditorGUILayout.EnumPopup("Public variable name notation", _setting.ScriptPublicVariableNameNotation);
			_setting.ScriptPrivateVariableNameNotation = (Notation)EditorGUILayout.EnumPopup("Private variable name notation", _setting.ScriptPrivateVariableNameNotation);
			_setting.ScriptPrivateVariableNamePrefix = EditorGUILayout.TextField("Private variable name prefix", _setting.ScriptPrivateVariableNamePrefix);
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Reference");
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginVertical("Box");
			_setting.ReferenceReplacementSymbol = EditorGUILayout.TextField("Replacement symbol", _setting.ReferenceReplacementSymbol);
			_setting.ReferenceReplacementWord = EditorGUILayout.TextField("Replacement word", _setting.ReferenceReplacementWord);
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
			_setting.UseDataContents = EditorGUILayout.Toggle("Use data contents", _setting.UseDataContents);
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
				var platform = (Platform)EditorGUILayout.EnumPopup("Platform", platformInfo.Platform);
				if (platformInfo.Platform != platform)
				{
					platformInfo.Platform = platform;
					switch (platform)
					{
						case Platform.Unity:
						{
							platformInfo.DefineName = "UNITY_2022_1_OR_NEWER";
							platformInfo.NamespaceNames = new List<string> { "UnityEngine", "UnityEngine.AddressableAssets" };
						}
							break;

						case Platform.CSharp:
						{
							platformInfo.DefineName = string.Empty;
							platformInfo.NamespaceNames.Clear();
						}
							break;

						default:
							throw new NotImplementedException();
					}
				}
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
						platformInfo.ScriptPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
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
						platformInfo.DataPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
				--EditorGUI.indentLevel;

				EditorGUILayout.Separator();
			}
			if (GUILayout.Button("Add Platform"))
			{
				_setting.PlatformInfos.Add(new PlatformInfo()
					{ NamespaceNames = new List<string> { "UnityEngine", "UnityEngine.AddressableAssets" } });
			}
			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;

			EditorGUILayout.EndVertical();
			--EditorGUI.indentLevel;
			EditorGUILayout.Separator();

			if (GUILayout.Button("Save"))
			{
				_information.Setting = _setting;
				EditorUtility.SetDirty(_information);
				AssetDatabase.SaveAssets();
				_mode = Mode.SheetList;
			}

			if (_information.IsValid() && GUILayout.Button("Back"))
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
