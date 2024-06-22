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
	/// 여러 개의 키를 사용하는 시트 예제 입니다.
	/// </summary>
	public sealed partial record MultipleKeySample : DesignSheetDataHelper<(int, int), MultipleKeySample>, IDesignSheetData<(int, int)>
	{
		[JsonIgnore]
		public (int, int) Key => (SkillId, Level);

		/// <summary> 고유 번호 </summary>
		public int SkillId { get; init; }

		/// <summary> 레벨 </summary>
		public int Level { get; init; }

		/// <summary> 공격력 계수 </summary>
		public int AttackPowerCoefficient { get; init; }

	}
}
