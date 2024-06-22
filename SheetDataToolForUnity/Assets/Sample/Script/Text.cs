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
	/// 텍스트키 관리 시트입니다.
	/// </summary>
	public sealed partial record Text : DesignSheetDataHelper<string, Text>, IDesignSheetData<string>
	{
		[JsonIgnore]
		public string Key => Id;

		/// <summary> 고유 식별자 </summary>
		public string Id { get; init; }

		/// <summary> 한국어 </summary>
		public string Korea { get; init; }

		/// <summary> 영어 </summary>
		public string English { get; init; }

	}
}
