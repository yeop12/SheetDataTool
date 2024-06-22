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
	/// 기본적인 Design 콘텐츠가 있는 시트 예시 입니다.
	/// </summary>
	public sealed partial record DesignSample : DesignSheetDataHelper<int, DesignSample>, IDesignSheetData<int>
	{
		[JsonIgnore]
		public int Key => Id;

		/// <summary> 고유 번호 </summary>
		public int Id { get; init; }

		/// <summary> 이름 텍스트 고유 번호 </summary>
		public string NameTextId { get; init; }

	}
}
