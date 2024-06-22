using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sheet
{
	public abstract record DesignSheetDataHelper<TKey, TValue> : BaseSheetDataHelper where TValue : IDesignSheetData<TKey>
	{
		private static Lazy<Dictionary<TKey, TValue>> _data = new(LoadData);

		public static IEnumerable<TValue> Data => _data.Value.Values;

		private static Dictionary<TKey, TValue> LoadData()
		{
			var json = ReadData<TValue>();

			if (JsonConvert.DeserializeObject<List<TValue>>(json) is not { } data)
			{
				throw new Exception($"{typeof(TValue).Name} does not loaded.");
			}
			return data.ToDictionary(x => x.Key, x => x);
		}

		public static async Task LoadDataAsync()
		{
			var json = await ReadDataAsync<TValue>();

			if (JsonConvert.DeserializeObject<List<TValue>>(json) is not { } data)
			{
				throw new Exception($"{typeof(TValue).Name} does not loaded.");
			}

			var dataDictionary = data.ToDictionary(x => x.Key, x => x);
			_data = new Lazy<Dictionary<TKey, TValue>>(dataDictionary);
		}

		public static TValue Find(TKey key, bool throwNotFoundException = false)
		{
			if (_data.Value.TryGetValue(key, out var result) is false && throwNotFoundException)
			{
				throw new ExcelDataNotFoundException<TValue>(key);
			}
			return result;
		}

		public static TValue Find( Predicate<TValue> match )
		{
			return _data.Value.Values.FirstOrDefault(match.Invoke);
		}

		public static IEnumerable<TValue> FindAll( Predicate<TValue> match ) 
		{
			return _data.Value.Values.Where(match.Invoke);
		}
	}
}
