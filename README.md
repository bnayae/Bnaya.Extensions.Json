# Bnaya Json Extensions  

[![Build & Deploy NuGet](https://github.com/bnayae/Bnaya.Extensions.Json/actions/workflows/Deploy.yml/badge.svg)](https://github.com/bnayae/Bnaya.Extensions.Json/actions/workflows/Deploy.yml)  

[![NuGet](https://img.shields.io/nuget/v/Bnaya.Extensions.Json.svg)](https://www.nuget.org/packages/Bnaya.Extensions.Json/) 

[![codecov](https://codecov.io/gh/bnayae/Bnaya.Extensions.Json/branch/main/graph/badge.svg?token=TPKF0JUWNT)](https://codecov.io/gh/bnayae/Bnaya.Extensions.Json)

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


<details><summary>Json</summary>
<blockquote>

``` json
{
  "friends": [
    {
      "name": "Yaron",    
      "id": 1
    },
    {
      "name": "Aviad",   
      "id": 2
    }
  ]
}
```

</blockquote>
</details>


<details><summary>Sample: `friends.[].name` or `friends.*.name`</summary>
<blockquote>

``` cs
var items = source.ToEnumerable("friends.[].name");
```
OR
``` cs
var items = source.ToEnumerable("friends.*.name");
```

RESULT
``` json
["Yaron", "Aviad"]
```

</blockquote>
</details>

<details><summary>Sample: `friends.[0].name`</summary>
<blockquote>

``` cs
var items = source.ToEnumerable("friends.[0].name");
```

RESULT
``` json
["Yaron"]
```

</blockquote>
</details>

<details><summary>Sample: `friends.[0].*`</summary>
<blockquote>

``` cs
var items = source.ToEnumerable("friends.[0].*");
```

RESULT
``` json
["Yaron",1]
```

</blockquote>
</details>

*See: YieldWhen_Path_Test in the source code*

### Predicate based enumeration

Yield element according to predicate and onMatch delegates.  

### Sample:

<details><summary>Json</summary>
<blockquote>

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

</blockquote>
</details>

<details><summary>Code</summary>
<blockquote>

``` cs
TraverseInstruction Predicate(JsonElement current, IImmutableList<string> breadcrumbs)
{
    if (breadcrumbs.Count < 4)
        return ToChildren;

    if (breadcrumbs[^4] == "relationship" &&
        breadcrumbs[^3] == "projects" &&
        breadcrumbs[^1] == "key")
    {
        return new TraverseInstruction(Stop, TraverseAction.Take);
    }

    return ToChildren;
}
var items = source.ToEnumerable(Predicate);
```

</blockquote>
</details>

---

## Filter

Reduce (or modify) a json

<details><summary>Json</summary>
<blockquote>

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

</blockquote>
</details>


<details><summary>Code</summary>
<blockquote>

``` cs

TraverseInstruction Strategy(
                JsonElement e,
                IImmutableList<string> breadcrumbs)
{ 
    if (e.ValueKind == JsonValueKind.Number)
    {
        var val = e.GetInt32();
        if (val > 30)
            return TraverseInstruction.TakeOrReplace;
        return TraverseInstruction.SkipToSibling;
    }
    if (e.ValueKind == JsonValueKind.Array || e.ValueKind == JsonValueKind.Object)
        return TraverseInstruction.ToChildren;
    return TraverseInstruction.TakeOrReplace;
}

JsonElement target = source.Filter(Strategy);
```
Will result in:
``` cs
{
  "B": [ { "Val": 40 }],
  "C": [ 50, 100 ],
  "Note": "Re-shape json"
}
```

</blockquote>
</details>

---

### Keep

*Path based Filter*


<details><summary>Json</summary>
<blockquote>
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

</blockquote>
</details>

<details><summary>Sample: B.*.val</summary>
<blockquote>

``` cs
var target = source.Keep("B.*.val");
```

RESULT

``` json
{"B":[{"Val":40},{"Val":20}]}
```

</blockquote>
</details>



<details><summary>Sample: B.[]</summary>
<blockquote>

``` cs
var target = source.Keep("B.[]");
```

RESULT

``` json
{"B":[{"Val":40},{"Val":20},{"Factor":20}]}
```

</blockquote>
</details>



<details><summary>Sample: B.[1].val</summary>
<blockquote>

``` cs
var target = source.Keep("B.[].Factor");
```

RESULT

``` json
{"B":[{"Factor":20}]}
```

</blockquote>
</details>

<details><summary>Sample: B.[1].val</summary>
<blockquote>

``` cs
var target = source.Keep("B.[1].val");
```

RESULT

``` json
{"B":[{"Val":20}]}
```

</blockquote>
</details>

---

## Remove

Remove elements from the json.

<details><summary>Json</summary>
<blockquote>

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

</blockquote>
</details>

<details><summary>Sample: B.[]</summary>
<blockquote>

> ``` cs
> var target = source.Remove("B.[]");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"C":[0,25,50,100],"Note":"Re-shape json"}
> ```

</blockquote>
</details>

<details><summary>Sample: B.*.val</summary>
<blockquote>

> ``` cs
> var target = source.Remove("B.*.val");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"B":[{"Factor":20}],"C":[0,25,50,100],"Note":"Re-shape json"}
> ```

</blockquote>
</details>

<details><summary>Sample: B.[1]</summary>
<blockquote>

> ``` cs
> var target = source.Remove("B.[1]");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"B":[{"Val":40},{"Factor":20}],"C":[0,25,50,100],"Note":"Re-shape json"}
> ```

</blockquote>
</details>

<details><summary>Sample: B</summary>
<blockquote>

> ``` cs
> var target = source.Remove("B");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"C":[0,25,50,100],"Note":"Re-shape json"}
> ```

</blockquote>
</details>

<details><summary>Sample: B.[1].val</summary>
<blockquote>

> ``` cs
> var target = source.Remove("B.[1].val");
> ```
> 
> RESULT
> 
> ``` json
> {"A":10,"B":[{"Val":40},{"Val":20},{"Factor":20}],"C":[0,25,50,100],"Note":"Re-shape json"}
> ```

</blockquote>
</details>

---

## TryAddProperty

Try to add property if missing.


<details><summary>Sample 1</summary>
<blockquote>

```json
{ "A": 0, "B": 0 }
```

```cs
source.RootElement.TryAddProperty("C", 1);
```

Result in:
```json
{ "A": 0, "B": 0, "C": 1 }
```

</blockquote>
</details>

<details><summary>Sample 2</summary>
<blockquote>

```json
{ "A": 0, "B": 0, "C": 0 }
```

```cs
source.RootElement.TryAddProperty("C", 1);
```

Result in:
```json
{ "A": 0, "B": 0, "C": 0 }
```

</blockquote>
</details>

<details><summary>Sample: 3</summary>
<blockquote>

```json
{ "A": 0, "B": 0, "C": null }
```

```cs
var source = JsonDocument.Parse(json);
source.RootElement.TryAddProperty("C", 1);
```

Result in:
```json
{ "A": 0, "B": 0, "C": 1 }
```

</blockquote>
</details>

*Unless sets the options not to ignore null*


<details><summary>Sample: ignored null</summary>
<blockquote>

*Ignored null*

```cs
var options = new JsonPropertyModificatonOpions
{
    IgnoreNull = false
};
var source = JsonDocument.Parse(json);
source.RootElement.TryAddProperty("C", 1);
```

Result in:
```json
{ "A": 0, "B": 0, "C": null }
```

</blockquote>
</details>


<details><summary>Sample: Changing property within a path</summary>
<blockquote>

*Changing property within a path*

```json
{
  "X": {
    "Y": {
    	"A": 0,
    	"B": 0
    }
  },
  "Y": { "Q": 2 }
}
```

```cs
source.RootElement.TryAddProperty("X.Y", "C", 1);
```

Result in:
```json
{
  "X": {
      "Y": {
          "A": 0,
          "B": 0,  
          "C": 1
      }
  },
  "Y": { "Q": 2 }
}
```

</blockquote>
</details>

--- 

## TryGetValue

Try to get a value within a path


<details><summary>Json</summary>
<blockquote>

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

</blockquote>
</details>

<details><summary>Sample: B.B1</summary>
<blockquote>

```cs
source.TryGetValue(out JsonElement value, "B.B1")
```

Result in:
```json
["Very","Cool"]
```

</blockquote>
</details>


<details><summary>Sample: B.B1.*</summary>
<blockquote>

```cs
source.TryGetValue(out JsonElement value, "B.B1.*")
```

Result in:
```json
Very
```

</blockquote>
</details>


## TryGet...

Try to get a value within a path.


<details><summary>Json</summary>
<blockquote>

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

</blockquote>
</details>

<details><summary>Sample: String</summary>
<blockquote>

```cs
source.TryGetString(out JsonElement value, "B.B2.*.B211")
```

Result in: `OK`

</blockquote>
</details>

<details><summary>Sample: DateTiemOffset</summary>
<blockquote>

```cs
source.TryGetValue(out JsonElement value, "B.*.B26")
```

Result in: `2007-09-01T10:35:01+02`

</blockquote>
</details>

<details><summary>Sample: Double</summary>
<blockquote>

```cs
source.TryGetNumber(out double value, "B.B2.B24")
```

Result in: `1.8`

</blockquote>
</details>

---

## Merge

Merging 2 or more json.
The last json will override previous on conflicts

<details><summary>Sample: 1</summary>
<blockquote>

Source: `{ "A": 1 }` ,  Merged:  `{ "B": 2 }`

```cs
var target = source.Merge(merged);
```

Result in: `{"A":1,"b":2}`

</blockquote>
</details>


<details><summary>Sample: 2</summary>
<blockquote>

Source: `{ "A": 1 }` ,  Merged:  `{"B":2,"C":3}`

```cs
var target = source.Merge(merged);
```

Result in: `{ "A":1, "B":2, "C":3 }`

</blockquote>
</details>


<details><summary>Sample: </summary>
<blockquote>

Source: `{ "A": 1 }` ,  Merged1:  `{"B":2}` , Merged2: `{"C":3}`

```cs
var target = source.Merge(merged1, merged2);
```

Result in: `{ "A":1, "B":2, "C":3 }`

</blockquote>
</details>


<details><summary>Sample: </summary>
<blockquote>

Source: `{ "A": 1 }` 

```cs
var target = source.MergeObject(new { b = 2 });
```

Result in: `{"A":1, "B":2}`

</blockquote>
</details>

### Merge Into

Merging json into specific path of a source json.  
> *The last json will override any previous one in case of a conflicts*

<details><summary>Sample: 1</summary>
<blockquote>

Source
``` json
{ "A": 1, "B": { "B1":[1, 2, 3] }, "C": { "D": { "X":1, "Y":2 } } }
```

```cs
var target = source.MergeInto("C.D.X", 100);
```

Result in: `{ "A": 1, "B": { "B1":[1, 2, 3] }, "C": { "D": { "X":100, "Y":2 } } }`


</blockquote>
</details>

<details><summary>Sample: 2</summary>
<blockquote>

Source
``` json
{ "A": 1, "B": { "B1":[1, 2, 3] }, "C": { "D": { "X":1, "Y":2 } } }
```

Merged
``` json
{ "Z": 3}
```

```cs
var target = source.MergeInto("C.D", merged);
```

Result in: `{ "A": 1, "B": { "B1":[1, 2, 3] }, "C": { "D": { "X":1, "Y":2, "Z": 3 } } }`


</blockquote>
</details>

<details><summary>Sample: 3</summary>
<blockquote>

Source
``` json
{'A':1,'B': {'B1':1}}
```

Merged
``` json
{'B1':3, 'C':[1,2,3]}
```

```cs
var target = source.MergeInto("B", merged);
```

Result in: `{'A':1, 'B':{'B1':3, 'C':[1,2,3]}}`


</blockquote>
</details>

<details><summary>Sample: 4</summary>
<blockquote>

Source
``` json
{'A':1,'B':{'B1':[1,2,3]}}
```

```cs
var target = source.MergeInto("B.B1.[1]", 5);
```

Result in: `{'A':1, 'B':{'B1':[1,5,3]}}`


</blockquote>
</details>

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

## Looking for other extensions?
Check the following
- [Bnaya.Extensions](https://github.com/bnayae/Bnaya.Extensions.Common)
- [Async extensions](https://github.com/bnayae/Bnaya.CSharp.AsyncExtensions)

