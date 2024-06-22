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
	/// Record Reference List 참조 시트 예시 입니다.
	/// </summary>
	public sealed partial record RecordReferenceListSample : DesignSheetDataHelper<int, RecordReferenceListSample>, IDesignSheetData<int>
	{
		/// <summary>
		/// 텍스트 정보
		/// </summary>
		[Serializable]
		public partial record TextInfo
		{
			/// <summary> 이름 텍스트 고유 번호 배열 </summary>
			[JsonProperty(nameof(TextIds))]
			private List<string> _textIds { get; init; } = new();

			[JsonIgnore]
			public IReadOnlyList<string> TextIds => _textIds;

			[JsonIgnore]
			private List<Text> _textReferences;

			[JsonIgnore]
			public IReadOnlyList<Text> TextReferences => _textReferences ??= TextIds.Select(x => Text.Find(x)).ToList();

		}

		[JsonIgnore]
		public int Key => Id;

		/// <summary> 고유 번호 </summary>
		public int Id { get; init; }

		/// <summary> 이름 텍스트 고유 번호 </summary>
		public TextInfo NameTextInfo { get; init; }

	}
}
