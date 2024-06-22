using System;
using System.Collections;
using System.Linq;
using UnityEditor;

namespace SheetDataTool
{
	[CustomEditor(typeof(Information))]
	public class InformationEditor : Editor
	{
		private Information _information;

		private void OnEnable()
		{
			_information = serializedObject.targetObject as Information;
		}

		public override void OnInspectorGUI()
		{
			var accessInfo = _information.AccessInfo;
			EditorGUILayout.LabelField("AccessInfo");
			EditorGUILayout.BeginVertical("Box");
			if (accessInfo is null)
			{
				EditorGUILayout.LabelField("None");
			}
			else
			{
				switch (accessInfo.SheetType)
				{
					case SheetType.ExcelSheet:
						EditorGUILayout.LabelField($"Path : {accessInfo.Path}");
						break;

					case SheetType.GoogleSheet:
						EditorGUILayout.LabelField($"Path : {accessInfo.Path}");
						EditorGUILayout.LabelField($"GoogleSheetId : {accessInfo.GoogleSheetId}");
						break;

					default:
						throw new NotImplementedException();
				}
			}
			EditorGUILayout.EndVertical();

			var setting = _information.Setting;
			EditorGUILayout.LabelField("Setting");
			EditorGUILayout.BeginVertical("Box");
			if (setting is null)
			{
				EditorGUILayout.LabelField("None");
			}
			else
			{
				DrawProperty(setting);
			}
			EditorGUILayout.EndVertical();
		}

		private static void DrawProperty(object targetObject)
		{
			var propertyInfos = targetObject.GetType().GetProperties();
			foreach (var propertyInfo in propertyInfos) 
			{
				if (propertyInfo.PropertyType.IsGenericType)
				{
					EditorGUILayout.LabelField(propertyInfo.Name);
					var values = (propertyInfo.GetValue(targetObject) as IList)!;
					++EditorGUI.indentLevel;
					for (var i = 0; i < values.Count; ++i)
					{
						var value = values[i];
						if (value.GetType().IsPrimitive || value is string)
						{
							EditorGUILayout.LabelField($"¦¦ {value}");
						}
						else
						{
							EditorGUILayout.LabelField($"{i + 1}. Element");
							EditorGUILayout.BeginVertical("Box");
							DrawProperty(value);
							EditorGUILayout.EndVertical();
						}
					}

					if (values.Count == 0)
					{
						EditorGUILayout.LabelField("None");
					}
					--EditorGUI.indentLevel;
				}
				else
				{
					var value = propertyInfo.GetValue(targetObject).ToString();
					EditorGUILayout.LabelField($"{propertyInfo.Name} : {value}");
				}
			}
		}
	}
}
