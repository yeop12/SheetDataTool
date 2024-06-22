using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sheet
{
	public abstract record ConstantSheetDataHelper<T> : BaseSheetDataHelper
	{
		protected static bool IsLoaded;

		protected static void LoadData()
		{
			IsLoaded = true;
			var json = ReadData<T>();

			if (JsonConvert.DeserializeObject<T>(json) is { }) return;
			throw new Exception($"{ typeof(T).Name} does not loaded.");
		}

		public static async Task LoadDataAsync()
		{
			IsLoaded = true;
			var json = await ReadDataAsync<T>();

			if (JsonConvert.DeserializeObject<T>(json) is { }) return;
			throw new Exception($"{ typeof(T).Name} does not loaded.");
		}
	}
}
