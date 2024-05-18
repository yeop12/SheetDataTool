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
			private readonly Dictionary<int, PropertyInfo> _extraItemInfoByColumn = new();
			private readonly SheetInfoView _sheetInfoView;

			public ElementItemAnalyzer( SheetInfoView sheetInfoView, Setting setting )
			{
				_sheetInfoView = sheetInfoView;

				var itemReflectionInfos = typeof(T).GetProperties()
					.Where(x => x.Name != "Row" && x.Name.EndsWith("Column") is false)
					.Select(x => new {PropertyInfo = x, Description= x.GetCustomAttribute<ContentsElementItemDescriptionAttribute>() ?? throw new ArgumentException($"{typeof(T)} type properties must have {nameof(ContentsElementItemDescriptionAttribute)}.")})
					.ToList();

				var rowProperty = typeof(T).GetProperty("Row");
				if (rowProperty is not null)
				{
					_extraItemInfoByColumn.Add(-1, rowProperty);
				}

				const int elementItemHeaderRow = 1;
				for (var column = 0; column < sheetInfoView.ColumnCount; ++column) 
				{
					var cell = sheetInfoView[elementItemHeaderRow, column];
					if (string.IsNullOrWhiteSpace(cell)) continue;
					var itemReflectionInfo =
						itemReflectionInfos.Find(x =>
							x.Description.Name.ChangeNotation(Notation.Pascal, setting.InputNotation) == cell) ??
						throw new InvalidContentsElementHeaderException(cell, sheetInfoView.GetRealRow(elementItemHeaderRow), sheetInfoView.GetRealColumn(column), typeof(T));
					_itemInfos.Add(new ItemInfo(column, itemReflectionInfo.Description, itemReflectionInfo.PropertyInfo));

					var columnProperty = typeof(T).GetProperty($"{itemReflectionInfo.Description.Name}Column");
					if (columnProperty is not null)
					{
						_extraItemInfoByColumn.Add(column, columnProperty);
					}
				}

				var essentialItemReflectionInfo = itemReflectionInfos.Find(x => x.Description.IsEssential && _itemInfos.All(y => x.Description.Name != y.Name));
				if(essentialItemReflectionInfo is not null)
				{
					throw new NotExistEssentialContentsElementHeaderException(essentialItemReflectionInfo.Description.Name,
						sheetInfoView.GetRealRow(0));
				}
			}

			public T GetElementItem(int row)
			{
				var result = Activator.CreateInstance<T>();

				if (_extraItemInfoByColumn.TryGetValue(-1, out var rowProperty))
				{
					rowProperty.SetValue(result, _sheetInfoView.GetRealRow(row));
				}

				foreach (var itemInfo in _itemInfos)
				{
					var cell = _sheetInfoView[row, itemInfo.Column];
					if (string.IsNullOrWhiteSpace(cell))
					{
						if (itemInfo.IsEssential && itemInfo.AllowsEmpty is false)
						{
							throw new NotExistEssentialContentsElement(_sheetInfoView.GetRealRow(row),
								_sheetInfoView.GetRealColumn(itemInfo.Column));
						}
						continue;
					}

					try
					{
						itemInfo.PropertyInfo.SetValue(result, TypeUtil.ChangeType(cell, itemInfo.PropertyInfo.PropertyType));
					}
					catch
					{
						throw new MismatchTypeException(itemInfo.PropertyInfo.PropertyType, cell, row, itemInfo.Column);
					}

					if (_extraItemInfoByColumn.TryGetValue(itemInfo.Column, out var columnProperty))
					{
						columnProperty.SetValue(result, _sheetInfoView.GetRealColumn(itemInfo.Column));
					}
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
