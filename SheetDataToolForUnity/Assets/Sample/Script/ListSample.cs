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
	/// List를 사용하는 시트 예시 입니다.
	/// </summary>
	public sealed partial record ListSample : DesignSheetDataHelper<int, ListSample>, IDesignSheetData<int>
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

		/// <summary>
		/// 이미지 정보
		/// </summary>
		[Serializable]
		public partial record ImageInfo
		{
			/// <summary> 아틀라스 이름 </summary>
			public string AtlasName { get; init; }

			/// <summary> 스프라이트 이름 </summary>
			public string SpriteName { get; init; }

		}

		[JsonIgnore]
		public int Key => Id;

		/// <summary> 고유 번호 </summary>
		public int Id { get; init; }

		/// <summary> 이름 텍스트 고유 번호 </summary>
		public string NameTextId { get; init; }

		/// <summary> 스킬 타입 </summary>
		public SkillType Type { get; init; }

		/// <summary> 아이콘 이미지 정보 </summary>
		public ImageInfo IconImageInfo { get; init; }

		/// <summary> 획득 후 소유할 수 있는 기간 </summary>
		public int? OwnedDays { get; init; }

		/// <summary> 사용 불가능한 맵 고유 번호 배열 </summary>
		[JsonProperty(nameof(RestrictedMapIds))]
		private List<int> _restrictedMapIds { get; init; } = new();

		[JsonIgnore]
		public IReadOnlyList<int> RestrictedMapIds => _restrictedMapIds;

	}
}
