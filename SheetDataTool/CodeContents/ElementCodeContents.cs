using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SheetDataTool
{
	internal abstract class ElementCodeContents<T> : CodeContents
	{
		private class ElementItemAnalyzer
		{
			private class ItemInfo
			{
				public int Column { get; }
				public string Name { get; }
				public bool IsEssential { get; }
				public bool AllowsEmpty { get; }
				public PropertyInfo PropertyInfo { get; }

				public ItemInfo(int column, ContentsElementItemDescriptionAttribute itemDescriptionAttribute, PropertyInfo propertyInfo)
				{
					Column = column;
					Name = itemDescriptionAttribute.Name;
					IsEssential = itemDescriptionAttribute.IsEssential;
					AllowsEmpty = itemDescriptionAttribute.AllowsEmpty;
					PropertyInfo = propertyInfo;
				}
			}

			private readonly List<ItemInfo> _itemInfos = new();
			private readonly SheetInfoView _sheetInfoView;

			public ElementItemAnalyzer( SheetInfoView sheetInfoView, Setting setting )
			{
				_sheetInfoView = sheetInfoView;

				var itemReflectionInfos = typeof(T).GetProperties()
					.Select(x => new {PropertyInfo = x, Description= x.GetCustomAttribute<ContentsElementItemDescriptionAttribute>() ?? throw new ArgumentException($"{typeof(T)} type properties must have {nameof(ContentsElementItemDescriptionAttribute)}.")})
					.ToList();
				
				const int elementItemHeaderRow = 1;
				for (var column = 0; column < sheetInfoView.ColumnCount; ++column) 
				{
					var cell = sheetInfoView[elementItemHeaderRow, column];
					if (string.IsNullOrWhiteSpace(cell)) continue;
					var itemReflectionInfo =
						itemReflectionInfos.Find(x =>
							x.Description.Name.ChangeNotation(Notation.Pascal, setting.InputNotation) == cell) ??
						throw new InvalidSheetRuleException($"'{cell}' does not belong to element item of '{typeof(T).FullName}'.", sheetInfoView.GetRealRow(elementItemHeaderRow), sheetInfoView.GetRealColumn(column));
					_itemInfos.Add(new ItemInfo(column, itemReflectionInfo.Description, itemReflectionInfo.PropertyInfo));
				}

				var essentialItemReflectionInfo = itemReflectionInfos.Find(x => x.Description.IsEssential && _itemInfos.All(y => x.Description.Name != y.Name));
				if(essentialItemReflectionInfo is not null)
				{
					throw new InvalidSheetRuleException($"{typeof(T).FullName} must contain '{essentialItemReflectionInfo.Description.Name}' element item.", sheetInfoView.GetRealRow(elementItemHeaderRow));
				}
			}

			public T GetElementItem(int row)
			{
				var result = Activator.CreateInstance<T>();

				foreach (var itemInfo in _itemInfos)
				{
					var cell = _sheetInfoView[row, itemInfo.Column];
					if (string.IsNullOrWhiteSpace(cell))
					{
						if (itemInfo.IsEssential && itemInfo.AllowsEmpty is false) 
						{
							throw new InvalidSheetRuleException($"'{itemInfo.Name}' element item is not allowed to be null.", _sheetInfoView.GetRealRow(row), _sheetInfoView.GetRealColumn(itemInfo.Column));
						}
						continue;
					}
					itemInfo.PropertyInfo.SetValue(result, TypeUtil.ChangeType(cell, itemInfo.PropertyInfo.PropertyType));
				}

				return result;
			}
		}

		private static List<T> GetElements(SheetInfoView sheetInfoView, Setting setting)
		{
			var result = new List<T>();
			var elementItemAnalyzer = new ElementItemAnalyzer(sheetInfoView, setting);

			const int elementItemDataStartRow = 2;
			for (var row = elementItemDataStartRow; row < sheetInfoView.RowCount; ++row) 
			{
				var firstCell = sheetInfoView[row, 0];
				if (setting.IsIgnoreCell(firstCell)) 
				{
					continue;
				}

				var item = elementItemAnalyzer.GetElementItem(row);
				result.Add(item);
			}
			return result;
		}

		protected List<T> Elements { get; }

		protected ElementCodeContents(SheetInfoView sheetInfoView, Setting setting) : base(sheetInfoView, setting)
		{
			Elements = GetElements(sheetInfoView, setting);
		}
	}
}
