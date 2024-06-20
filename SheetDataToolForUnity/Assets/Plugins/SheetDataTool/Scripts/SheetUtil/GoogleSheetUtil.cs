using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace SheetDataTool
{
	public class GoogleSheetUtil : SheetUtil
	{
		private IList<Sheet> _sheets;
		private readonly SheetsService _service;
		private readonly string _spreadSheetID;

		public GoogleSheetUtil( string oauthJsonPath, string spreadSheetID)
		{
			var scopes = new[] { SheetsService.Scope.Spreadsheets };
			const string applicationName = "GoogleSheetForUnity";
			using var stream = new FileStream(oauthJsonPath, FileMode.Open);
			var credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
			_service = new SheetsService(new BaseClientService.Initializer() 
			{
				HttpClientInitializer = credential,
				ApplicationName = applicationName,
			});

			_spreadSheetID = spreadSheetID;
			RefreshSheetList();
		}

		public GoogleSheetUtil( Stream oauthStream, string spreadSheetID)
		{
			var scopes = new[] { SheetsService.Scope.Spreadsheets };
			const string applicationName = "GoogleSheetForUnity";
			var credential = GoogleCredential.FromStream(oauthStream).CreateScoped(scopes);
			_service = new SheetsService(new BaseClientService.Initializer() 
			{
				HttpClientInitializer = credential,
				ApplicationName = applicationName,
			});

			var spreadSheet = _service.Spreadsheets.Get(spreadSheetID).Execute();
			_sheets = spreadSheet.Sheets;
			_spreadSheetID = spreadSheetID;
		}

		public IEnumerable<string> GetSheetNames() => _sheets.Select(x => x.Properties.Title);

		public SheetInfo GetSheetInfo(string sheetName)
		{
			var valueRange = _service.Spreadsheets.Values.Get(_spreadSheetID, sheetName).Execute();
			if (valueRange is null) return new SheetInfo(sheetName, new string?[0, 0]);

			var rowCount = valueRange.Values.Count;
			var columnCount = valueRange.Values.Max(x => x.Count);
			var data = new string?[rowCount, columnCount];
			for (var row = 0; row < valueRange.Values.Count; ++row)
			{
				var rowData = valueRange.Values[row];
				for (var column = 0; column < rowData.Count; ++column)
				{
					var cell = rowData[column]?.ToString();
					data[row, column] = cell;
				}
			}

			return new SheetInfo(sheetName, data);
		}

		public void RefreshSheetList()
		{
			var spreadSheet = _service.Spreadsheets.Get(_spreadSheetID).Execute();
			_sheets = spreadSheet.Sheets;
		}

		public string GetPath(string sheetName) => _spreadSheetID;
	}
}
