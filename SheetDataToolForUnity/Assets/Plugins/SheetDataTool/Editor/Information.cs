using Newtonsoft.Json;
using UnityEngine;

namespace SheetDataTool
{
	[CreateAssetMenu(fileName = "Information", menuName = "SheetDataTool/Information")]
	public class Information : ScriptableObject
	{
		[SerializeField] [HideInInspector] private string _accessInfoJson;
		[SerializeField] private string _settingJson;

		public AccessInfo AccessInfo
		{
			get => string.IsNullOrWhiteSpace(_accessInfoJson) ? null :JsonConvert.DeserializeObject<AccessInfo>(_accessInfoJson);
			set => _accessInfoJson = JsonConvert.SerializeObject(value);
		}

		public Setting Setting
		{
			get => string.IsNullOrWhiteSpace(_settingJson) ? null : JsonConvert.DeserializeObject<Setting>(_settingJson);
			set => _settingJson = JsonConvert.SerializeObject(value);
		}

		public bool IsValid() => string.IsNullOrWhiteSpace(_accessInfoJson) is false &&
		                         string.IsNullOrWhiteSpace(_settingJson) is false;
	}
}
