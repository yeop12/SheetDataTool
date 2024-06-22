using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if UNITY_2020_1_OR_NEWER
using UnityEngine;
using UnityEngine.AddressableAssets;
#endif

namespace Sheet
{
	public abstract record BaseSheetDataHelper
	{
		#if UNITY_2020_1_OR_NEWER
		protected static readonly string DefaultDirectory = "SheetData";
		#else
		protected static readonly string DefaultDirectory = string.Empty;
		#endif

		protected static string ReadData<T>()
		{
			#if UNITY_2020_1_OR_NEWER
			var textAsset = Addressables.LoadAssetAsync<TextAsset>($"{DefaultDirectory}/{typeof(T).Name}").WaitForCompletion();
			var json = textAsset.text;
			#else
			var json = string.Empty;
			#endif

			return json;
		}

		protected static async Task<string> ReadDataAsync<T>()
		{
			#if UNITY_2020_1_OR_NEWER
			var textAsset = await Addressables.LoadAssetAsync<TextAsset>($"{DefaultDirectory}/{typeof(T).Name}").Task;
			var json = textAsset.text;
			#else
			var json = string.Empty;
			#endif

			return json;
		}

		public static async Task LoadAllDataAsync()
		{
			var sheetTypes = typeof(BaseSheetDataHelper).Assembly.GetTypes().Where(x =>x.IsAbstract is false && x.IsSubclassOf(typeof(BaseSheetDataHelper)));
			var methodInfos = sheetTypes.Select(x => x.GetMethod("LoadDataAsync", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public));
			var loadFunctions = methodInfos.Select(x => x?.Invoke(null, new object[] { })).Cast<Task>();
			await Task.WhenAll(loadFunctions);
		}
	}
}
