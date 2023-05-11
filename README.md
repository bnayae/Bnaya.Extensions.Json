# Weknow Json Extensions  

[![Prepare](https://github.com/weknow-network/Weknow-Json-Extensions/actions/workflows/prepare-nuget.yml/badge.svg)](https://github.com/weknow-network/Weknow-Json-Extensions/actions/workflows/prepare-nuget.yml) [![Build & Deploy NuGet](https://github.com/weknow-network/Weknow-Json-Extensions/actions/workflows/Deploy.yml/badge.svg)](https://github.com/weknow-network/Weknow-Json-Extensions/actions/workflows/Deploy.yml)  
[![NuGet](https://img.shields.io/nuget/v/Weknow.Text.Json.Extensions.svg)](https://www.nuget.org/packages/Weknow.Text.Json.Extensions/)  

Functionality of this library includes:

- [ToEnumerable](#ToEnumerable)
- [Filter](#Filter)
- [Keep](#Keep)
- [Remove](#Remove)
- [TryAddProperty](#TryAddProperty)
- [TryGetValue](#TryGetValue)
- [TryGet...](#tryget)
- [Merge](#Merge)
  - [Merge Into](#Merge-Into)
- [Serialization](#Serialization)
  - [Convert Object into Json Element](#ToJson)
  - [Convert Json Element to string](#AsString)
  - [Convert Json Element to Stream](#ToStream)

---

## ToEnumerable 

Enumerate over json elements.

*Useful for fetching selected parts of the json* 

### Path based enumeration

Rely on path convention:

`json.ToEnumerable(/path convention/);`

JSON:
>``` json
>{
>  "friends": [
>    {
>      "name": "Yaron",    
>      "id": 1
>    },
>    {
>      "name": "Aviad",   
>      "id": 2
>    }
>  ]
>}
>```

CODE: 
> ``` cs
> var items = source.ToEnumerable("friends.[].name");
> ```
> OR
> ``` cs
> var items = source.ToEnumerable("friends.*.name");
> ```
> 
> RESULT
> ``` json
> ["Yaron", "Aviad"]
> ```

CODE: 
> ``` cs
> var items = source.ToEnumerable("");
> ```
> OR
> ``` cs
> var items = source.ToEnumerable("");
> ```
> 
> RESULT
> ``` json
> [
> ```

CODE: 
> ``` cs
> var items = source.ToEnumerable("friends.[0].name");
> ```
> 
> RESULT
> ``` json
> ["Yaron"]
> ```

CODE: 
> ``` cs
> var items = source.ToEnumerable("friends.[0].*");
> ```
> OR
> ``` cs
> var items = source.ToEnumerable("");
> ```
> 
> RESULT
> ``` json
> ["Yaron",1]
> ```

*See: YieldWhen_Path_Test in the source code*

### Yield with predicate

Yield element according to predicate and onMatch delegates.  

JSON:
``` json
{
    "projects": [ {
        "key": "cloud-d",
        "duration": 8
    }, {
    "users": [{
        "rank": {
            "job": 10,
            "relationship": {
                "projects": [
                    { "key": "cloud-d", "team": 5 },
                    { "key": "cloud-x", "team": 32 }
                ]
            }
        },
    }]
}
```

### Sample:
> ``` cs
> using static System.Text.Json.TraverseFlowInstruction;
> 
> var items = source.ToEnumerable((json, deep, breadcrumbs) =>
> {
>     if(breadcrumbs.Count < 4)
>         return Drill;
> 
>     if (breadcrumbs[^4] == "relationship" &&
>         breadcrumbs[^3] == "projects" &&
>         breadcrumbs[^1] == "key")
>     {
>         //return Drill;
>         return Yield;
>     }
> 
>     return Drill;
> });
> 
> // "cloud-d", "cloud-x"
> var results = items.Select(m => m.GetString()).ToArray();
> ```

---

## Filter

Reduce (or modify) the json

JSON:
``` json
{
  "A": 10,
  "B": [
    { "Val": 40 },
    { "Val": 20 },
    { "Factor": 20 }
  ],
  "C": [0, 25, 50, 100 ],
  "Note": "Re-shape json"
}
```

### Sample:
> ``` cs
> JsonElement source = ..;
> var target = source.Filter((e, _, _) =>
>             e.ValueKind != JsonValueKind.Number || e.GetInt32() > 30 
>             ? TraverseFlowWrite.Drill 
>             : TraverseFlowWrite.Skip);
> ```
> Will result in:
> ``` cs
> {
>   "B": [ { "Val": 40 }],
>   "C": [ 50, 100 ],
>   "Note": "Re-shape json"
> }
> ```

---

### Keep

*Path based Filter*

JSON:
``` json
{
  "A": 10,
  "B": [
    { "Val": 40 },
    { "Val": 20 },
    { "Factor": 20 }
  ],
  "C": [0, 25, 50, 100 ],
  "Note": "Re-shape json"
}
```

### Sample 1:

> ``` cs
> var target = source.Keep("B.*.val");
> ```
> 
> RESULT
>
> ``` json
> {"B":[{"Val":40},{"Val":20}]}
> ```

### Sample 2:

> ``` cs
> var target = source.Keep("B.[]");
> ```
> 
> RESULT
> 
> ``` json
> {"B":[{"Val":40},{"Val":20},{"Factor":20}]}
> ```

### Sample 3:

> ``` cs
> var target = source.Keep("B.[].Factor");
> ```
> 
> RESULT
> 
> ``` json
> {"B":[{"Factor":20}]}
> ```

### Sample 4:

> ``` cs
> var target = source.Keep("B.[1].val");
> ```
> 
> RESULT
> 
> ``` json
> {"B":[{"Val":20}]}
> ```

---

## Remove

Remove elements from the json.

``` json
{
  "A": 10,
  "B": [
    { "Val": 40 },
    { "Val": 20 },
    { "Factor": 20 }
  ],
  "C": [0, 25, 50, 100 ],
  "Note": "Re-shape json"
}
```

### Sample 1:

> ``` cs
> var target = source.Remove("");
> ```
> 
> RESULT
> 
> ``` json
> 
> ```

### Sample 2:

> ``` cs
> var target = source.Remove("B.[]");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"B":[],"C":[],"Note":"Re-shape json"}
> ```

### Sample 3:

> ``` cs
> var target = source.Remove("B.*.val");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"B":[{"Factor":20}],"C":[0,25,50,100],"Note":"Re-shape json"}
> ```

### Sample 4:

> ``` cs
> var target = source.Remove("B.[1]");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"B":[{"Val":40},{"Factor":20}],"C":[0,50,100],"Note":"Re-shape json"}
> ```

---

## TryAddProperty

Try to add property if missing.

### Sample 1

> ```json
> { "A": 0, "B": 0 }
> ```
> 
> ```cs
> source.RootElement.TryAddProperty("C", 1);
> ```
> 
> Result in:
> ```json
> { "A": 0, "B": 0, "C": 1 }
> ```

### Sample 2

> ```json
> { "A": 0, "B": 0, "C": 0 }
> ```
> 
> ```cs
> source.RootElement.TryAddProperty("C", 1);
> ```
> 
> Result in:
> ```json
> { "A": 0, "B": 0, "C": 0 }
> ```


### Sample 3

> ```json
> { "A": 0, "B": 0, "C": null }
> ```
> 
> ```cs
> var source = JsonDocument.Parse(json);
> source.RootElement.TryAddProperty("C", 1);
> ```
> 
> Result in:
> ```json
> { "A": 0, "B": 0, "C": 1 }
> ```

*Unless sets the options not to ignore null*

### Sample 3 

> *Ignored null*
> 
> ```cs
> var options = new JsonPropertyModificatonOpions
> {
>     IgnoreNull = false
> };
> var source = JsonDocument.Parse(json);
> source.RootElement.TryAddProperty("C", 1);
> ```
> 
> Result in:
> ```json
> { "A": 0, "B": 0, "C": null }
> ```

### Sample 4

> *Changing property within a path*
> 
> ```json
> {
>   "X": {
>     "Y": {
>     	"A": 0,
>     	"B": 0
>     }
>   },
>   "Y": { "Q": 2 }
> }
> ```
> 
> ```cs
> source.RootElement.TryAddProperty("X.Y", "C", 1);
> ```
> 
> Result in:
> ```json
> {
>   "X": {
>       "Y": {
>           "A": 0,
>           "B": 0,  
>           "C": 1
>       }
>   },
>   "Y": { "Q": 2 }
> }
> ```

--- 

## TryGetValue

Try to get a value within a path

``` json
{
  "B": {
    "B1": ["Very", "Cool"],
    "B2": {
        "B21": {
          "B211": "OK"
        },
        "B22": 22,   
        "B23": true,
        "B24": 1.8,
        "B25": "2007-09-01T10:35:01",
        "B26": "2007-09-01T10:35:01+02"
    }
  }
}
```

### Sample 1 

> ```cs
> source.TryGetValue(out JsonElement value, "B.B1")
> ```
> 
> Result in:
> ```json
> ["Very","Cool"]
> ```

### Sample 2 

> ```cs
> source.TryGetValue(out JsonElement value, "B.B1.*")
> ```
> 
> Result in:
> ```json
> Very
> ```


## TryGet...

Try to get a value within a path.


``` json
{
  "B": {
    "B1": ["Very", "Cool"],
    "B2": {
        "B21": {
          "B211": "OK"
        },
        "B22": 22,   
        "B23": true,
        "B24": 1.8,
        "B25": "2007-09-01T10:35:01",
        "B26": "2007-09-01T10:35:01+02"
    }
  }
}
```

### String 

> ```cs
> source.TryGetString(out JsonElement value, "B.B2.*.B211")
> ```
> 
> Result in: `OK`

### DateTiemOffset 

> ```cs
> source.TryGetValue(out JsonElement value, "B.*.B26")
> ```
> 
> Result in: `2007-09-01T10:35:01+02`

### Double

> ```cs
> source.TryGetNumber(out double value, "B.B2.B24")
> ```
> 
> Result in: `1.8`

---

## Merge

Merging 2 or more json.
The last json will override previous on conflicts

###  Sample 1

> Source: `{ "A": 1 }` ,  Merged:  `{ "B": 2 }`
> 
> ```cs
> var target = source.Merge(merged);
> ```
> 
> Result in: `{"A":1,"b":2}`

###  Sample 2

> Source: `{ "A": 1 }` ,  Merged:  `{"B":2,"C":3}`
> 
> ```cs
> var target = source.Merge(merged);
> ```
> 
> Result in: `{ "A":1, "B":2, "C":3 }`

###  Sample 3

> Source: `{ "A": 1 }` ,  Merged1:  `{"B":2}` , Merged2: `{"C":3}`
> 
> ```cs
> var target = source.Merge(merged1, merged2);
> ```
> 
> Result in: `{ "A":1, "B":2, "C":3 }`

###  Sample 4

> Source: `{ "A": 1 }` 
> 
> ```cs
> var target = source.MergeObject(new { b = 2 });
> ```
> 
> Result in: `{"A":1, "B":2}`

### Merge Into

Merging json into specific path of a source json.  
> *The last json will override any previous one in case of a conflicts*

Source

``` json
{ "A": 1, "B": { "B1":[1, 2, 3] } }
```

###  Sample 1

> Source
> ``` json
> { "C": { "D": { "X":1, "Y":2 } } }
> ```
>
> Merged
> ``` json
> { "Z": 3 }
> ```
> 
> ```cs
> var target = source.MergeInto("C.D", merged);
> ```
> 
> Result in: `{ "C": { "D": { "X":1, "Y":2, "Z": 3 } } }`


###  Sample 2

> Merged
> ``` json
> { 5 }
> ```
>
> ```cs
> var target = source.MergeInto("B.B1.[1]", merged);
> ```
> 
> Result in: `{ "A": 1, "B": { "B1":[1, 5, 3] } }`


###  Sample 3

> Source
> ``` json
> { "C": { "D": { "X":1, "Y":2 } } }
> ```
> 
> Merged
> ``` json
> {  "Z": 3 }
> ```
>
> ```cs
> var target = source.MergeInto("C.D", merged);
> ```
> 
> Result in: `{ "C": { "D": { "X":1, "Y":2, "Z": 3 } } }`


###  Sample 4

> ```cs
> var target = source.MergeInto("B.B1.[1]", 5);
> ```
> 
> Result in: `{ "A": 1, "B": { "B1":[1, 5, 3] } }`


###  Sample 5

> ```cs
> var merged = new { New = "Object"}; // anonymous type
> var target = source.MergeInto("B.B1.[1]", merged);
> ```
> 
> Result in: `{ "A": 1, "B": { "B1":[1, {"New":"Object"}, 3] }`

###  Sample 6

> Source
> ``` json
> { "C": { "D": { "X":1, "Y":2 } } }
> ```
> 
> ```cs
> var target = source.MergeInto("C.D", new { Z = 3 });
> ```
> 
> Result in: `{ "C": { "D": { "X":1, "Y":2, "Z": 3 } } }`

## Serialization

### ToJson

Convert .NET object into JsonElement.

``` cs
var entity = new Entity(12, new BEntity("Z"));
JsonElement json = entity.ToJson();
```

``` cs
var arr = new []{ 1, 2, 3 };
JsonElement json = arr.ToJson();
```

### AsString

Convert JsonElement to string

``` cs
JsonElement json = ...;
string compact = json.AsString();
string indented = json.AsIndentString();
string raw = json.GetRawText(); // same as json.AsIndentString();
```

### ToStream

Convert JsonElement to stream 

``` cs
var json = JsonDocument.Parse(JSON);
var srm = json.ToStream() as MemoryStream;
string result = Encoding.UTF8.GetString(srm.ToArray()) ;
```

