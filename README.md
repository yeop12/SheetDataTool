# Table Of Contents
* [Introduction](#Introduction)
* [Installation](#Installation)
* [Sheet Guide](#Sheet-Guide)
* [Unity Guide](#Unity-Guide)
* [Release Notes](#Release-Notes)
* [License](#License)

# Introduction
SheetDataTool easily processes GoogleSheet or ExcelSheet data, serializes it to json, and automatically generates code for use in Unity.

# Installation
## Dependencies
    https://github.com/Cysharp/UniTask

## Install via git URL
1. Open package manager

![image](https://github.com/yeop12/SheetDataTool/assets/11326612/4d3ee0b2-7194-409d-868e-06d940c7d42e)
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/a01d7101-8305-45ff-89e0-63f381ad579b)


2. Add package from git URL

![image](https://github.com/yeop12/SheetDataTool/assets/11326612/73946711-20a9-4f9a-9b73-9278bca91531)

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
public sealed partial record Item : DesignSheetDataHelper<int, Item>, IDesignSheetData<int>
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
public sealed partial record Item : DesignSheetDataHelper<int, Item>, IDesignSheetData<int>
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
public sealed partial record Item : DesignSheetDataHelper<int, Item>, IDesignSheetData<int>
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
public sealed partial record Item : DesignSheetDataHelper<int, Item>, IDesignSheetData<int>
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
public sealed partial record Item : DesignSheetDataHelper<int, Item>, IDesignSheetData<int>
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
public sealed partial record Item : FullSheetDataHelper<int, Item>, IDesignSheetData<int>
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
## Generate google sheet oauth token  
<details>
<summary>Contents</summary>

1. Enter google cloud platform site(https://console.cloud.google.com/)  
2. Select project button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/7ceb0f91-6487-4121-a134-7b0c575eaf0b)  
3. Select new prject button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/539c91f5-5495-48bd-bf8c-fbec4142d887)  
4. Create new project  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/35d137c3-2661-4d5b-b186-36954a578291)  
5. Select api and service then click aouth button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/25c32850-f8aa-4757-b03f-8c514bd0ec72)  
6. Click created project  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/3e5e0add-a4d6-4f42-9511-e188be512bc3)  
7. Click outside and create  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/db2177ed-7557-4054-b3bd-fcb58e52a70a)  
8. Write information and continue  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/ecfd5afb-5b06-42ea-9964-c67b65ac5677)  
9. Save and continue  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/b230b31e-3ac1-42ec-b156-16cf1b6817c6)  
10. Save and continue  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/b7fc2ba4-b798-4e1e-86fd-e20d89f47c6f)  
11. Back to dashboard  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/ce08b9fd-7b50-46ba-90f5-9efcd67f5ed0)  
12. Click IAM then service account button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/a1a330f1-fc92-43bb-825f-57118aa2dcb6)  
13. Click create new service account button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/cf26b82d-3603-4dbf-8fc6-289759053dc9)  
14. Click confirm button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/aef39d87-4416-4480-975b-efe5bcef808f)  
15. Click email button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/8c617db5-dbf4-4d6f-8594-99cdc0abed83)  
16. Click key and new key button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/b1ebef1c-f151-4614-8b1a-86295eee219d)  
17. Create with json  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/257a121d-2699-4096-93bb-dfbe72502c6b)  
18. Click product and solution then all product button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/c0e2e553-0569-4d03-9e8a-7968b9edbe16)  
19. Search 'sheet'  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/462e44db-b51c-43e0-a5c6-2d0ca7acac6e)  
20. Click google sheet api button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/14b9b00c-3936-44a7-84f4-ed5736190c2b)  
21. Click use button  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/9bc92b3e-5603-495f-97d7-63edf85ee3cc)  

</details>

## Set up window setting  
1. Open SheetDataTool window(Window -> SheetDataTool)  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/1045edfb-bd5b-4060-87dd-522c3574026a)

2. Write access info  
First select sheet type  
GoogleSheet : google sheet in cloud  
ExcelSheet : excel sheet file in drive  
* Google Sheet
OAuth file path : Set it to the path of the aouth file created in the previous step.  
Spread sheet id : Write down the value in the path after d/ among the paths in the spread sheet.  
(Ex, If path is https://docs.google.com/spreadsheets/d/123456789A then 123456789A is id.)  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/e898ea82-aaf2-43ad-85db-8d7daa40c1f1)  
  
* Excel Sheet
Select excel file folder path  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/92a0bf16-6708-44f3-bbc7-b759e72fb565)  

3. Set setting values  
Set the setting information to be used in sheets and scripts.  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/e74ea58f-72a9-4045-9984-378ea4c742c3)  
	
Enter the platform information to use. One script and data file per platform is provided and supports Unity or general csharp.  
When using Unity, you must add UnityEngine and UnityEngine.AddressableAssets.    
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/284fec7a-a243-491f-bc86-3db317da11f1)  
	
4. Generate data and script  
When the setting is complete, the list of sheets is displayed and data and scripts are created by pressing the cs and ed buttons.  
cs : regenerate script and json file(when data structure changed.)  
ed : export only json file(when data structure not changed.)  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/8ef5bb06-85aa-446a-8ddf-0653652c44ad)  

5. Generate default script  
Create a basic script for using the sheet data tool.  
![image](https://github.com/yeop12/SheetDataTool/assets/11326612/01cf0131-807a-4946-aa5b-8588b316853e)  
  
## How to use  
```
// 1. Find member by key
var key = 1;
var item = Item.Find(key);

// 2. Find member by condition
var item = Item.Find(x => x.Name == "Sword");
	
// 3. Find members by condition
var items = Item.FindAll(x => x.Type == ItemType.Potion);
	
// 4. Call all items
foreach(var item in Item.Data)
{
	// something...
}
var items = Item.Data.Where(x => x.Type == ItemType.Potion); // same way as 3

// 5. Call constant value
var maxItemCount = Item.MaxItemCount;
```
	
# Release Notes
## Version 1.0.1
* Bug fixes

# License
This library is under the MIT License.
