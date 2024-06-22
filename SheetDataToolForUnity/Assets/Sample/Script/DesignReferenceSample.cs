using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#if UNITY_2020_1_OR_NEWER
using UnityEngine;
using UnityEngine.AddressableAssets;
#endif

namespace Sheet
{
	/// <summary>
	/// Design Reference 참조 시트 예시 입니다.
	/// </summary>
	public sealed partial record DesignReferenceSample : DesignSheetDataHelper<int, DesignReferenceSample>, IDesignSheetData<int>
	{
		[JsonIgnore]
		public int Key => Id;

		/// <summary> 고유 번호 </summary>
		public int Id { get; init; }

		/// <summary> 이름 텍스트 고유 번호 </summary>
		public string NameTextId { get; init; }

		[JsonIgnore]
		private Text _nameTextReference;

		[JsonIgnore]
		public Text NameTextReference => _nameTextReference ??= Text.Find(NameTextId);

	}
}
