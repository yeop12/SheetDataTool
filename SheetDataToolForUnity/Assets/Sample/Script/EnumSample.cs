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
	/// Enum 콘텐츠를 사용하는 시트 예시 입니다.
	/// </summary>
	public sealed partial record EnumSample : DesignSheetDataHelper<int, EnumSample>, IDesignSheetData<int>
	{
		/// <summary>
		/// 스킬 타입
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public enum SkillType : int
		{
			/// <summary> 액티브 스킬 </summary>
			Active,

			/// <summary> 패시브 스킬 </summary>
			Passive,

		}

		[JsonIgnore]
		public int Key => Id;

		/// <summary> 고유 번호 </summary>
		public int Id { get; init; }

		/// <summary> 이름 텍스트 고유 번호 </summary>
		public string NameTextId { get; init; }

		/// <summary> 스킬 타입 </summary>
		public SkillType Type { get; init; }

	}
}
