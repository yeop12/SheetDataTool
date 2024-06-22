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
	/// Design Reference List 참조 시트 예시 입니다.
	/// </summary>
	public sealed partial record DesignReferenceListSample : DesignSheetDataHelper<int, DesignReferenceListSample>, IDesignSheetData<int>
	{
		[JsonIgnore]
		public int Key => Id;

		/// <summary> 고유 번호 </summary>
		public int Id { get; init; }

		/// <summary> 이름 텍스트 고유 번호 배열 </summary>
		[JsonProperty(nameof(NameTextIds))]
		private List<string> _nameTextIds { get; init; } = new();

		[JsonIgnore]
		public IReadOnlyList<string> NameTextIds => _nameTextIds;

		[JsonIgnore]
		private List<Text> _nameTextReferences;

		[JsonIgnore]
		public IReadOnlyList<Text> NameTextReferences => _nameTextReferences ??= NameTextIds.Select(x => Text.Find(x)).ToList();

	}
}
