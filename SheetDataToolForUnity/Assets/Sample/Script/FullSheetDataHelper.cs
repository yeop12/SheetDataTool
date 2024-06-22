using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sheet
{
	public abstract record FullSheetDataHelper<TKey, TValue> : BaseSheetDataHelper where TValue : IDesignSheetData<TKey>
	{
		protected static bool IsLoaded;
		private static Dictionary<TKey, TValue> _data;
		public static IEnumerable<TValue> Data
		{
			get
			{
				if (IsLoaded is false) LoadData();
				return _data.Values;
			}
		}

		protected static void LoadData()
		{
			IsLoaded = true;
			var json = ReadData<TValue>();

			var (_ ,data) = JsonConvert.DeserializeObject<(TValue, List<TValue>)>(json);
			if(data is null)
			{
				throw new Exception($"{typeof(TValue).Name} does not loaded.");
			}
			_data = data.ToDictionary(x => x.Key, x => x);
		}

		public static async Task LoadDataAsync()
		{
			IsLoaded = true;
			var json = await ReadDataAsync<TValue>();

			var (_ ,data) = JsonConvert.DeserializeObject<(TValue, List<TValue>)>(json);
			if(data is null)
			{
				throw new Exception($"{typeof(TValue).Name} does not loaded.");
			}
			_data = data.ToDictionary(x => x.Key, x => x);
		}

		public static TValue Find(TKey key, bool throwNotFoundException = false)
		{
			if (IsLoaded is false) LoadData();
			if (_data.TryGetValue(key, out var result) is false && throwNotFoundException)
			{
				throw new ExcelDataNotFoundException<TValue>(key);
			}
			return result;
		}

		public static TValue Find( Predicate<TValue> match )
		{
			if (IsLoaded is false) LoadData();
			return _data.Values.FirstOrDefault(match.Invoke);
		}

		public static IEnumerable<TValue> FindAll( Predicate<TValue> match ) 
		{
			if (IsLoaded is false) LoadData();
			return _data.Values.Where(match.Invoke);
		}
	}
}
