# Table Of Contents
* Introduction
* Installation
* Sheet Guide
* Unity Guide
* Release Notes
* License

# Introduction
SheetDataTool easily processes GoogleSheet or ExcelSheet data, serializes it to json, and automatically generates code for use in Unity.

# Installation
## Dependencies
    https://github.com/Cysharp/UniTask

## Install via git URL
1. Open package manager
2. Add package from git URL
```
https://github.com/yeop12/SheetDataTool.git?path=SheetDataToolForUnity/Assets/Plugins/SheetDataTool
```

# Sheet Guide
The sheet guide explains how to create a sheet, given each situation and tells you how to create a sheet for it.

## Make basic sheet
### Example situation
Let's first design an item table. The item table has unique number and name information.

### Sheet
|**Description**| | | |
|---|---|---|---|
|Item table|
| |
|**Design**|
|Name|Type|IsPrimaryKey|Comment|
|Id|int|TRUE|Unique identifier|
|Name|string| |Name|
| |
|**Name**|
|Id|Name|
| |
|**Data**|
|1|Sword|
|2|RedPotion|

To create a sheet, each item must be defined. Basically, Description, Design, Name, and Data items must be defined, and each content is as follows.
* Description : Write a description of the table.
* Design : Write a table structue.

Design sub items
|Item|ForceExist|Information|
|---|---|---|
|Name|O|Item's name|
|Type|O|Item's type|
|IsPrimaryKey|O|Indicates whether the item is a unique key. If set as a unique key, it cannot have the same value|
|Comment|X|Item's comment|

* Name : Represents a column of items in a table structure.
* Data : Write a table data.

### Generated Code
```
/// <summary>
/// Item table
/// </summary>
public sealed partial record Temp : DesignSheetDataHelper<int, Temp>, IDesignSheetData<int>
{
	[JsonIgnore]
	public int Key => Id;

	/// <summary> Unique identifier </summary>
	public int Id { get; init; }

	/// <summary> Name </summary>
	public string Name { get; init; }

}
```

### Serialized Data(Json)
```
[
  {
    "Id": 1,
    "Name": "Sword"
  },
  {
    "Id": 2,
    "Name": "RedPotion"
  }
]
```

## Usable types
Sheets can use C# basic types, Unity built-in types, List, and user-defined types, and can declare them as Nullable.

### C# basic types
|Name|Description|
|---|---|
|sbyte|signed 8-bit interger|
|byte|unsigned 8-bit interger|
|short|signed 16-bit interger|
|ushort|unsigned 16-bit interger|
|int|signed 32-bit interger|
|uint|unsigned 32-bit interger|
|long|signed 64-bit interger|
|ulong|usinged 64-bit interger|
|float|32-bit single-precision|
|double|64-bit double-precision|
|string|string literals|
|bool|logical uantities|
|List| |

### Unity built-in types
|Name|Description|
|---|---|
|Vector2|2 dimension float vector|
|Vector3|3 dimention float vector|
|Vector2Int|2 dimension int vector|
|Vector3Int|3 dimension int vector|
|Color|Represents a color value and has red, blue, green, and alpha values|

### Declare as Nullable   
You can define a nullable type by adding a ? after the type.

## Using nullable type
### Example situation
Next, let's use a nullable type. Add the usage period to the item table, and use null if there is no value if the usage period exists.

### Sheet
|**Description**| | | |
|---|---|---|---|
|Item table|
| |
|**Design**|
|Name|Type|IsPrimaryKey|Comment|
|Id|int|TRUE|Unique identifier|
|Name|string| |Name|
|`Period`|`int?`||`Possession period(Days)`|
| |
|**Name**|
|Id|Name|`Period`|
| |
|**Data**|
|1|Sword|`200`|
|2|RedPotion|`null`|

### Generated Code
```
/// <summary>
/// Item table
/// </summary>
public sealed partial record Temp : DesignSheetDataHelper<int, Temp>, IDesignSheetData<int>
{
	[JsonIgnore]
	public int Key => Id;

	/// <summary> Unique identifier </summary>
	public int Id { get; init; }

	/// <summary> Name </summary>
	public string Name { get; init; }

	/// <summary> Possession period(Days) </summary>
	public int? Period { get; init; }

}
```

### Serialized Data(Json)
```
[
  {
    "Id": 1,
    "Name": "Sword",
    "Period": 200
  },
  {
    "Id": 2,
    "Name": "RedPotion",
    "Period": null
  }
]
```

## Using enum
### Example situtation
Next, let's add the item type to the table. Item types include weapon, potion, and package types, and let's mark them as enum.

### Sheet
|**Description**| | | |
|---|---|---|---|
|Item table|
| |
|**`Enum:Global`**|`ItemType`|
|`Name`|`Comment`|
|`Weapon`|`Weapon item`|
|`Potion`|`Potion item`|
|`Package`|`Package item`|
| |
|**Design**|
|Name|Type|IsPrimaryKey|Comment|
|Id|int|TRUE|Unique identifier|
|Name|string| |Name|
|Period|int?| |Possession period(Days)|
|`Type`|`ItemType`| |`Item type`|
| |
|**Name**|
|Id|Name|Period|Type|
| |
|**Data**|
|1|Sword|200|`Weapon`|
|2|RedPotion|null|`Potion`|
|`3`|`BeginnerPackage`|`null`|`Package`|

To use enum, enum items must be defined and names and names of each sub item must be written.

#### Enum sub items
|Item|ForceExist|Information
|---|---|---|
|Name|O|Item's name|
|Comment|X|Item's comment|
|Value|X|Item's value(If the value is set, it is set to that value, and if not, it has the previous value +1)|

Options : Each item can have options, separated by colon(:).

#### Enum options
|Option|Information|
|---|---|
|Global|By defining the corresponding enum globally, it can be used in other tables as well|
|Types(long, ulong, int, uint, short, ushort, sbyte, byte)|Enum type|


### Generated Code
```
[JsonConverter(typeof(StringEnumConverter))]
public enum ItemType : int
{
	/// <summary> Weapon item </summary>
	Weapon,

	/// <summary> Potion item </summary>
	Potion,

	/// <summary> Package item </summary>
	Package,

}

/// <summary>
/// Item table
/// </summary>
public sealed partial record Temp : DesignSheetDataHelper<int, Temp>, IDesignSheetData<int>
{
	[JsonIgnore]
	public int Key => Id;

	/// <summary> Unique identifier </summary>
	public int Id { get; init; }

	/// <summary> Name </summary>
	public string Name { get; init; }

	/// <summary> Possession period(Days) </summary>
	public int? Period { get; init; }

	/// <summary> Item type </summary>
	public ItemType Type { get; init; }

}
```

### Serialized Data(Json)
```
[
  {
    "Id": 1,
    "Name": "Sword",
    "Period": 200,
    "Type": "Weapon"
  },
  {
    "Id": 2,
    "Name": "RedPotion",
    "Period": null,
    "Type": "Potion"
  },
  {
    "Id": 3,
    "Name": "BeginnerPackage",
    "Period": null,
    "Type": "Package"
  }
]
```

## Using list
### Example situation
Next, let's add a package item. A package item is an item that bundles various items, and has a list of item numbers owned by it.

### Sheet
|**Description**| | | | | |
|---|---|---|---|---|---|
|Item table|
| |
|**Enum:Global**|ItemType|
|Name|Comment|
|Weapon|Weapon item|
|Potion|Potion item|
|Package|Package item|
| |
|**Design**|
|Name|Type|IsPrimaryKey|Comment|
|Id|int|TRUE|Unique identifier|
|Name|string| |Name|
|Period|int?| |Possession period(Days)|
|Type|ItemType| |Item type|
|`PackageItemIds`|`List<int>`| |`If it is a package item, list of package contents item unique number`|
| |
|**Name**|
|Id|Name|Period|Type|`PackageItemIds[0]`|`PackageItemIds[1]`|
| | | | |
|**Data**|
|1|Sword|200|Weapon|
|2|RedPotion|null|Potion|
|3|BeginnerPackage|null|Package|`1`|`2`|

To use a list, declare it as List<T> when the desired type is T, and write the index of each list name in the Name item. If the list item value of Data is empty, it is regarded as having no value, and only when there is a value, the value is entered into the corresponding list.

### Generated Code
```
/// <summary>
/// Item table
/// </summary>
public sealed partial record Temp : DesignSheetDataHelper<int, Temp>, IDesignSheetData<int>
{
	[JsonIgnore]
	public int Key => Id;

	/// <summary> Unique identifier </summary>
	public int Id { get; init; }

	/// <summary> Name </summary>
	public string Name { get; init; }

	/// <summary> Possession period(Days) </summary>
	public int? Period { get; init; }

	/// <summary> Item type </summary>
	public ItemType Type { get; init; }

	/// <summary> If it is a package item, list of package contents item unique number </summary>
	[JsonProperty(nameof(PackageItemIds))]
	private List<int> _packageItemIds { get; init; }

	[JsonIgnore]
	public IReadOnlyList<int> PackageItemIds => _packageItemIds;

}
```

### Serizlied Data
```
[
  {
    "Id": 1,
    "Name": "Sword",
    "Period": 200,
    "Type": "Weapon",
    "PackageItemIds": null
  },
  {
    "Id": 2,
    "Name": "RedPotion",
    "Period": null,
    "Type": "Potion",
    "PackageItemIds": null
  },
  {
    "Id": 3,
    "Name": "BeginnerPackage",
    "Period": null,
    "Type": "Package",
    "PackageItemIds": [
      1,
      2
    ]
  }
]
```

## Using record
### Example situation
Next, let's add item icon image information to the item table. Item image consists of Atlas name and Sprite name, and let's define it by binding it to record.

### Sheet
|**Description**| | | | | | | |
|---|---|---|---|---|---|---|---|
|Item table|
| |
|**Enum:Global**|ItemType|
|Name|Comment|
|Weapon|Weapon item|
|Potion|Potion item|
|Package|Package item|
| |
|**`Record:Global`**|`ImageInfo`|
|`Name`|`Type`|`Comment`|
|`SpriteName`|`string`|`Sprite name for image`|
|`AtlasName`|`string`|`Atlas name for image`|
| |
|**Design**|
|Name|Type|IsPrimaryKey|Comment|
|Id|int|TRUE|Unique identifier|
|Name|string| |Name|
|Period|int?| |Possession period(Days)|
|Type|ItemType| |Item type|
|PackageItemIds|List<int>| |If it is a package item, list of package contents item unique number|
|`IconImageInfo`|`ImageInfo`| |`Icon image info`|
| |
|**Name**|
|Id|Name|Period|Type|PackageItemIds[0]|PackageItemIds[1]|`IconImageInfo.SpriteName`|`IconImageInfo.AtlasName`|
| |
|**Data**|
|1|Sword|200|Weapon| | |`Sword`|`WeaponAtlas`|
|2|RedPotion|null|Potion| | |`RedPotion`|`PotionAtlas`|
|3|BeginnerPackage|null|Package|1|2|`BeginnerPackage`|`PackageAtlas`|

To use the record item, define the record, name on the right side, and name, type, and comment corresponding to the sub item on the bottom. Also, in the Name item, you can access each record item by adding a . to the name of each design.

#### Record Options
|Option|Information|
|---|---|
|Global|By defining the corresponding record globally, it can be used in other tables as well|

### Generated Code
```
[Serializable]
public partial record ImageInfo
{
	/// <summary> Sprite name for image </summary>
	public string SpriteName { get; init; }

	/// <summary> Atlas name for image </summary>
	public string AtlasName { get; init; }

}

/// <summary>
/// Item table
/// </summary>
public sealed partial record Temp : DesignSheetDataHelper<int, Temp>, IDesignSheetData<int>
{
	[JsonIgnore]
	public int Key => Id;

	/// <summary> Unique identifier </summary>
	public int Id { get; init; }

	/// <summary> Name </summary>
	public string Name { get; init; }

	/// <summary> Possession period(Days) </summary>
	public int? Period { get; init; }

	/// <summary> Item type </summary>
	public ItemType Type { get; init; }

	/// <summary> If it is a package item, list of package contents item unique number </summary>
	[JsonProperty(nameof(PackageItemIds))]
	private List<int> _packageItemIds { get; init; }

	[JsonIgnore]
	public IReadOnlyList<int> PackageItemIds => _packageItemIds;

	/// <summary> Icon image info </summary>
	public ImageInfo IconImageInfo { get; init; }

}
```

### Serizlied Data(Json)
```
[
  {
    "Id": 1,
    "Name": "Sword",
    "Period": 200,
    "Type": "Weapon",
    "PackageItemIds": null,
    "IconImageInfo": {
      "SpriteName": "Sword",
      "AtlasName": "WeaponAtlas"
    }
  },
  {
    "Id": 2,
    "Name": "RedPotion",
    "Period": null,
    "Type": "Potion",
    "PackageItemIds": null,
    "IconImageInfo": {
      "SpriteName": "RedPotion",
      "AtlasName": "PotionAtlas"
    }
  },
  {
    "Id": 3,
    "Name": "BeginnerPackage",
    "Period": null,
    "Type": "Package",
    "PackageItemIds": [
      1,
      2
    ],
    "IconImageInfo": {
      "SpriteName": "BeginnerPackage",
      "AtlasName": "PackageAtlas"
    }
  }
]
```

## Using constant
### Example situation
Next, we define the maximum number of items we can have as a constant.

### Sheet
|**Description**| | | | | | | |
|---|---|---|---|---|---|---|---|
|Item table|
| |
|**`Constant`**|
|`Name`|`Type`|`Value`|`Comment`|
|`MaxItemCount`|`int`|`20`|`Maximum number of items a player can have`|
| |
|**Enum:Global**|ItemType|
|Name|Comment|
|Weapon|Weapon item|
|Potion|Potion item|
|Package|Package item|
| |
|**Record:Global**|ImageInfo|
|Name|Type|Comment|
|SpriteName|string|Sprite name for image|
|AtlasName|string|Atlas name for image|
| |
|**Design**|
|Name|Type|IsPrimaryKey|Comment|
|Id|int|TRUE|Unique identifier|
|Name|string| |Name|
|Period|int?| |Possession period(Days)|
|Type|ItemType| |Item type|
|PackageItemIds|List<int>| |If it is a package item, list of package contents item unique number|
|IconImageInfo|ImageInfo|Icon image info|
| |
|**Name**|
|Id|Name|Period|Type|PackageItemIds[0]|PackageItemIds[1]|IconImageInfo.SpriteName|IconImageInfo.AtalsName|
| |
|**Data**|
|1|Sword|200|Weapon| | |Sword|WeaponAtlas|
|2|RedPotion|null|Potion| | |RedPotion|PotionAtlas|
|3|BeginnerPackage|null|Package|1|2|BeginnerPackage|PackageAtlas|

### Generated Code
```
/// <summary>
/// Item table
/// </summary>
public sealed partial record Temp : FullSheetDataHelper<int, Temp>, IDesignSheetData<int>
{
	[JsonProperty(nameof(MaxItemCount))]
	private static int _maxItemCount { get; set; }

	/// <summary> Maximum number of items a player can have </summary>
	[JsonIgnore]
	public static int MaxItemCount
	{
		get
		{
			if(IsLoaded is false) LoadData();
			return _maxItemCount;
		}
	}


	[JsonIgnore]
	public int Key => Id;

	/// <summary> Unique identifier </summary>
	public int Id { get; init; }

	/// <summary> Name </summary>
	public string Name { get; init; }

	/// <summary> Possession period(Days) </summary>
	public int? Period { get; init; }

	/// <summary> Item type </summary>
	public ItemType Type { get; init; }

	/// <summary> If it is a package item, list of package contents item unique number </summary>
	[JsonProperty(nameof(PackageItemIds))]
	private List<int> _packageItemIds { get; init; }

	[JsonIgnore]
	public IReadOnlyList<int> PackageItemIds => _packageItemIds;

	/// <summary> Icon image info </summary>
	public ImageInfo IconImageInfo { get; init; }

}
```

### Serialized Data(Json)
```
{
"item1" : 
{
  "MaxItemCount": 20
},
"item2" : 
[
  {
    "Id": 1,
    "Name": "Sword",
    "Period": 200,
    "Type": "Weapon",
    "PackageItemIds": null,
    "IconImageInfo": {
      "SpriteName": "Sword",
      "AtlasName": "WeaponAtlas"
    }
  },
  {
    "Id": 2,
    "Name": "RedPotion",
    "Period": null,
    "Type": "Potion",
    "PackageItemIds": null,
    "IconImageInfo": {
      "SpriteName": "RedPotion",
      "AtlasName": "PotionAtlas"
    }
  },
  {
    "Id": 3,
    "Name": "BeginnerPackage",
    "Period": null,
    "Type": "Package",
    "PackageItemIds": [
      1,
      2
    ],
    "IconImageInfo": {
      "SpriteName": "BeginnerPackage",
      "AtlasName": "PackageAtlas"
    }
  }
]
}
```

## Using interface
Update later.

# Unity Guide
## Generate goolge sheet aouth token

## Set up window setting

## Generate data and script

# How it works?

# Release Notes
## Version 1.0.1
* Bug fixes

# License
This library is under the MIT License.
