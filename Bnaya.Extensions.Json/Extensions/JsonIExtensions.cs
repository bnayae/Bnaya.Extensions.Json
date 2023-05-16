using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;

using Bnaya.Extensions.Json.deprecated;

using static System.Text.Json.Extension.Constants;

// credit: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to

using static System.Text.Json.TraverseInstruction;
using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseMarkSemantic;
using System.Collections.Concurrent;

namespace System.Text.Json;


/// <summary>
/// Json extensions
/// </summary>
public static partial class JsonExtensions
{
    private static JsonWriterOptions INDENTED_JSON_OPTIONS = new JsonWriterOptions { Indented = true };

    #region Keep

    /// <summary>
    /// Rewrite json while excluding elements which doesn't match the path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="onMatch"><![CDATA[
    /// Notify when find a match.
    /// According to the return value it will replace or remove the element.
    /// Replaced when returning alternative `JsonElement` otherwise Removed.
    /// 
    /// Action's signature : (current, deep, breadcrumbs) => ...;]]></param>
    /// <returns></returns>
    public static JsonElement Keep(
        this JsonDocument source,
        string path,
        bool caseSensitive = false,
        JsonMatchHook? onMatch = null)
    {
        return source.RootElement.Keep(path, caseSensitive, onMatch);
    }


    /// <summary>
    /// Rewrite json while excluding elements which doesn't match the path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="caseSensitive">indicate whether path should be a case sensitive</param>
    /// <param name="onMatch"><![CDATA[
    /// Notify when find a match.
    /// According to the return value it will replace or remove the element.
    /// Replaced when returning alternative `JsonElement` otherwise Removed.
    /// 
    /// Action's signature : (current, deep, breadcrumbs) => ...;]]></param>
    /// <returns></returns>
    public static JsonElement Keep(
        this in JsonElement source,
        string path,
        bool caseSensitive = false,
        JsonMatchHook? onMatch = null)
    {
        TraversePredicate predicate =
            CreatePathPredicate(path, caseSensitive);
        return source.Filter(predicate, onMatch);
    }

    #endregion // Keep

    #region Remove

    /// <summary>
    /// Rewrite json while excluding elements according to a path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="caseSensitive">indicate whether path should be a case sensitive</param>
    /// <returns></returns>
    public static JsonElement Remove(
        this JsonDocument source,
        string path,
        bool caseSensitive = false)
    {
        return source.RootElement.Remove(path, caseSensitive);
    }

    /// <summary>
    /// Rewrite json while excluding elements according to a path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="caseSensitive">indicate whether path should be a case sensitive</param>
    /// <returns></returns>
    public static JsonElement Remove(
        this in JsonElement source,
        string path,
        bool caseSensitive = false)
    {
        TraversePredicate predicate =
            CreatePathPredicate(path, caseSensitive, Ignore);
        return source.Filter(predicate);
    }

    #endregion // Remove

    #region AsString

    /// <summary>
    /// Gets the json representation as string.
    /// </summary>
    /// <param name="json">The j.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static string AsString(
        this JsonDocument json,
        JsonWriterOptions options = default)
    {
        return json.RootElement.AsString(options);
    }

    /// <summary>
    /// Gets the json representation as string.
    /// </summary>
    /// <param name="json">The j.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static string AsString(
        this JsonElement json,
        JsonWriterOptions options = default)
    {
        if (json.ValueKind == JsonValueKind.String) return json.GetString() ?? String.Empty;
        if (json.ValueKind == JsonValueKind.Number) return $"{json.GetDouble()}";
        if (json.ValueKind == JsonValueKind.True) return "False";
        if (json.ValueKind == JsonValueKind.False) return "False";
        if (json.ValueKind == JsonValueKind.Null || json.ValueKind == JsonValueKind.Undefined)
            return String.Empty;
        using var ms = new MemoryStream();
        using (var w = new Utf8JsonWriter(ms, options))
        {
            json.WriteTo(w);
        }
        var result = Encoding.UTF8.GetString(ms.ToArray());
        return result;
    }

    /// <summary>
    /// Gets the json representation as indented string.
    /// </summary>
    /// <param name="json">The j.</param>
    /// <returns></returns>
    public static string AsIndentString(
        this JsonDocument json)
    {
        return json.RootElement.AsString(INDENTED_JSON_OPTIONS);
    }

    /// <summary>
    /// Gets the json representation as indented string.
    /// </summary>
    /// <param name="json">The j.</param>
    /// <returns></returns>
    public static string AsIndentString(
        this JsonElement json)
    {
        return json.AsString(INDENTED_JSON_OPTIONS);
    }

    #endregion // AsString

    #region ToStream

    /// <summary>
    /// Gets the json representation as Stream.
    /// </summary>
    /// <param name="json">The j.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static Stream ToStream(
        this JsonDocument json,
        JsonWriterOptions options = default)
    {
        return json.RootElement.ToStream(options);
    }

    /// <summary>
    /// Gets the json representation as string.
    /// </summary>
    /// <param name="json">The j.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static Stream ToStream(
        this JsonElement json,
        JsonWriterOptions options = default)
    {
        var ms = new MemoryStream();
        using (var w = new Utf8JsonWriter(ms, options))
        {
            json.WriteTo(w);
        }
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    #endregion // ToStream

    #region Serialize

    /// <summary>
    /// Serializes the specified instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static string Serialize<T>(
        this T instance,
        JsonSerializerOptions? options = null)
    {
        options = options ?? SerializerOptions;
        string json = JsonSerializer.Serialize(instance, options);
        return json;
    }

    #endregion // Serialize

    #region ToJson

    /// <summary>
    /// Convert instance to JsonElement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="options">The options (used for the serialization).</param>
    /// <returns></returns>
    public static JsonElement ToJson<T>(
        this T instance,
        JsonSerializerOptions? options = null)
    {
        if (instance is JsonElement element) return element;
        if (instance is JsonDocument doc) return doc.RootElement;

        options = options ?? SerializerOptions;
        byte[]? j = JsonSerializer.SerializeToUtf8Bytes(instance, options);
        if (j == null)
            return CreateEmptyJsonElement();
        var json = JsonDocument.Parse(j);
        return json.RootElement;
    }

    #endregion // ToJson

    #region TraverseProps

    /// <summary>
    /// TraversePropsProps over a json, will callback when find a property.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="propNames">The property name.</param>
    /// <param name="action">Action: (writer, current-element, current-path)</param>
    /// <exception cref="System.NotSupportedException">Only 'Object' or 'Array' element are supported</exception>
    [Obsolete("Consider to be removed, try the Filter API", false)]
    public static JsonElement TraverseProps(
        this in JsonElement element,
        Action<Utf8JsonWriter, JsonProperty, IImmutableList<string>> action,
        params string[] propNames)
    {
        var set = ImmutableHashSet.CreateRange(propNames);
        return TraverseProps(element, set, action);
    }

    /// <summary>
    /// TraversePropsProps over a json, will callback when find a property.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="propNames">The property name.</param>
    /// <param name="action">Action: (writer, current-element, current-path)</param>
    /// <exception cref="System.NotSupportedException">Only 'Object' or 'Array' element are supported</exception>
    [Obsolete("Consider to be removed, try the Filter API", true)]
    public static JsonElement TraverseProps(
        this in JsonElement element,
        IImmutableSet<string> propNames,
        Action<Utf8JsonWriter, JsonProperty, IImmutableList<string>> action)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            element.TraverseProps(writer, propNames, action, 0);
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan);
        JsonDocument result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;
    }

    /// <summary>
    /// TraversePropsProps over a json, will callback when find a property.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="writer">The positive writer.</param>
    /// <param name="propNames">The property name.</param>
    /// <param name="action">Action: (writer, current-element, current-path)</param>
    /// <param name="deepLimit">The recursive deep (0 = ignores, 1 = only root elements).</param>
    /// <param name="breadcrumbs"></param>
    /// <exception cref="System.NotSupportedException">Only 'Object' or 'Array' element are supported</exception>
    private static void TraverseProps(
        this in JsonElement element,
        Utf8JsonWriter writer,
        IImmutableSet<string> propNames,
        Action<Utf8JsonWriter, JsonProperty, IImmutableList<string>> action,
        byte deepLimit = 0,
        IImmutableList<string>? breadcrumbs = null)
    {
        IImmutableList<string> spine = breadcrumbs ?? ImmutableList<string>.Empty;
        int curDeep = spine.Count;
        if (deepLimit != 0 && curDeep >= deepLimit)
        {
            element.WriteTo(writer);
            return;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();

            foreach (JsonProperty e in element.EnumerateObject())
            {
                var curSpine = spine.Add(e.Name);
                bool isEquals = propNames.Contains(e.Name);
                JsonElement v = e.Value;
                if (isEquals)
                {
                    action(writer, e, curSpine);
                }
                else if (v.ValueKind == JsonValueKind.Object)
                {
                    WriteRec();
                }
                else if (v.ValueKind == JsonValueKind.Array)
                {
                    WriteRec();
                }
                else
                {
                    Write();
                }

                #region Local Methods: Write(), WriteRec()


                void WriteRec()
                {

                    writer.WritePropertyName(e.Name);

                    v.TraverseProps(writer, propNames, action, deepLimit, curSpine);
                }

                void Write()
                {
                    writer.WritePropertyName(e.Name);
                    v.WriteTo(writer);
                }

                #endregion // Local Methods: Write(), WriteRec()
            }
            writer.WriteEndObject();
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            int i = 0;
            foreach (JsonElement e in element.EnumerateArray())
            {
                var curSpine = spine.Add($"[{i++}]");
                e.TraverseProps(writer, propNames, action,
                               deepLimit,
                               curSpine);
            }
            writer.WriteEndArray();
        }
        else
        {
            element.WriteTo(writer);
        }
    }

    #endregion // TraverseProps

    #region IntoProp

    /// <summary>
    /// Wrap Into a property of empty element.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="propName">Name of the property.</param>
    /// <returns></returns>
    public static JsonElement IntoProp(this in JsonElement source, string propName)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WritePropertyName(propName);
            source.WriteTo(writer);
            writer.WriteEndObject();
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan);
        JsonDocument result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;
    }

    #endregion // IntoProp

    #region AddIntoArray

    /// <summary>
    /// Adds the addition into existing json array.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="addition">The addition into the source.</param>
    /// <returns></returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public static JsonElement AddIntoArray<T>(this JsonDocument source, params T[] addition) => AddIntoArray(source.RootElement, addition);

    /// <summary>
    /// Adds the addition into existing json array.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="addition">The addition into the source.</param>
    /// <returns></returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public static JsonElement AddIntoArray<T>(
                                this in JsonElement source,
                                params T[] addition)
    {
        if (addition.Length == 1)
            return source.AddIntoArray(addition[0].ToJson());
        return source.AddIntoArray(addition.Select(m => m.ToJson()).ToJson());
    }

    /// <summary>
    /// Adds the addition into existing json array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="options">The options.</param>
    /// <param name="addition">The addition into the source.</param>
    /// <returns></returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public static JsonElement AddIntoArray<T>(this in JsonElement source,
        JsonSerializerOptions options, params T[] addition)
    {
        if (addition.Length == 1)
            return source.AddIntoArray(addition[0].ToJson(options));
        return source.AddIntoArray(addition.Select(m => m.ToJson()).ToJson());
    }

    /// <summary>
    /// Adds the addition into existing json array.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="addition">The addition into the source.</param>
    /// <param name="deconstruct">if set to <c>true</c> will be merged into the source (deconstruct).</param>
    /// <returns></returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public static JsonElement AddIntoArray(this JsonDocument source, JsonElement addition, bool deconstruct = true) => AddIntoArray(source.RootElement, addition, deconstruct);

    /// <summary>
    /// Adds the addition into existing json array.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="addition">The addition into the source.</param>
    /// <param name="deconstruct">if set to <c>true</c> will be merged into the source (deconstruct).</param>
    /// <returns></returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public static JsonElement AddIntoArray(this in JsonElement source, JsonElement addition, bool deconstruct = true)
    {
        if (source.ValueKind != JsonValueKind.Array)
            throw new NotSupportedException("Only array are eligible as source");
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartArray();
            foreach (var item in source.EnumerateArray())
            {
                item.WriteTo(writer);
            }
            if (addition.ValueKind == JsonValueKind.Array && deconstruct)
            {
                foreach (var item in addition.EnumerateArray())
                {
                    item.WriteTo(writer);
                }
            }
            else
                addition.WriteTo(writer);
            writer.WriteEndArray();
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan);
        JsonDocument result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;
    }

    #endregion // AddIntoArray

    #region TryGetValue

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetValue(
                            this JsonDocument source,
                            out JsonElement value,
                            string path)
    {
        return source.RootElement.TryGetValue(out value, path);
    }

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetValue(
                            this in JsonElement source,
                            out JsonElement value,
                            string path)
    {
        JsonElement? target = source.ToEnumerable(path).FirstOrDefault();
        value = target ?? CreateEmptyJsonElement();
        return target != null;
    }

    #endregion // TryGetValue

    #region TryGetString

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetString(
                            this JsonDocument source,
                            out string value,
                            string path)
    {
        return source.RootElement.TryGetString(out value, path);
    }

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetString(
                            this in JsonElement source,
                            out string value,
                            string path)
    {
        JsonElement? target = source.ToEnumerable(path).FirstOrDefault();
        if (target?.ValueKind != JsonValueKind.String)
        {
            value = string.Empty;
            return false;
        }
        value = target?.GetString() ?? string.Empty;
        return target != null;
    }

    #endregion // TryGetString

    #region TryGetDateTimeOffset

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetDateTimeOffset(
                            this JsonDocument source,
                            out DateTimeOffset value,
                            string path)
    {
        return source.RootElement.TryGetDateTimeOffset(out value, path);
    }

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetDateTimeOffset(
                            this in JsonElement source,
                            out DateTimeOffset value,
                            string path)
    {
        JsonElement? target = source.ToEnumerable(path).FirstOrDefault();
        value = default;

        if (target == null)
            return false;
        if (target?.ValueKind != JsonValueKind.String)
            return false;
        if (target?.TryGetDateTimeOffset(out value) ?? false)
            return true;
        value = default;
        return false;
    }

    #endregion // TryGetDateTimeOffset

    #region TryGetDateTime

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetDateTime(
                            this JsonDocument source,
                            out DateTime value,
                            string path)
    {
        return source.RootElement.TryGetDateTime(out value, path);
    }

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetDateTime(
                            this in JsonElement source,
                            out DateTime value,
                            string path)
    {
        JsonElement? target = source.ToEnumerable(path).FirstOrDefault();
        value = default;

        if (target == null)
            return false;
        if (target?.ValueKind != JsonValueKind.String)
            return false;
        if (target?.TryGetDateTime(out value) ?? false)
            return true;
        value = default;
        return false;
    }

    #endregion // TryGetDateTime

    #region TryGetBoolean

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetBoolean(
                            this JsonDocument source,
                            out bool value,
                            string path)
    {
        return source.RootElement.TryGetBoolean(out value, path);
    }

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetBoolean(
                            this in JsonElement source,
                            out bool value,
                            string path)
    {
        JsonElement? target = source.ToEnumerable(path).FirstOrDefault();
        if (target?.ValueKind != JsonValueKind.True &&
            target?.ValueKind != JsonValueKind.False)
        {
            value = default;
            return false;
        }
        value = target?.GetBoolean() ?? default;
        return target != null;
    }

    #endregion // TryGetBoolean

    #region TryGetNumber

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetNumber<T>(
                            this JsonDocument source,
                            out T value,
                            string path)
        where T : INumberBase<T>
    {
        return source.RootElement.TryGetNumber<T>(out value, path);
    }

    /// <summary>
    /// Get a value from a path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="value">When this method returns, contains the value of the specified path.</param>
    /// <param name="path">The name's path of the property to find.</param>
    /// <returns></returns>
    public static bool TryGetNumber<T>(
                            this in JsonElement source,
                            out T value,
                            string path)
        where T : notnull, INumberBase<T>
    {
        JsonElement? target = source.ToEnumerable(path).FirstOrDefault();
#pragma warning disable CS8601 
        value = default;
#pragma warning restore CS8601 

        if (target?.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (value is int)
        {
            var res = target.Value.TryGetInt32(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is double)
        {
            var res = target.Value.TryGetDouble(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is float)
        {
            var res = target.Value.TryGetSingle(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is byte)
        {
            var res = target.Value.TryGetByte(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is sbyte)
        {
            var res = target.Value.TryGetSByte(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is decimal)
        {
            var res = target.Value.TryGetDecimal(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is ushort)
        {
            var res = target.Value.TryGetUInt16(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is short)
        {
            var res = target.Value.TryGetInt16(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is uint)
        {
            var res = target.Value.TryGetUInt32(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is long)
        {
            var res = target.Value.TryGetInt64(out var v);
            value = T.CreateChecked(v);
            return res;
        }
        if (value is ulong)
        {
            var res = target.Value.TryGetUInt64(out var v);
            value = T.CreateChecked(v);
            return res;
        }

        return false;
    }

    #endregion // TryGetNumber

    #region TryAddProperty

    /// <summary>
    /// Try add property into a specific path in the json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source json.</param>
    /// <param name="path">The path where the property should go into.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static JsonElement TryAddProperty<T>(
                                this in JsonElement source,
                                string path,
                                string name,
                                T value,
                                JsonPropertyModificatonOpions? options = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            return source.TryAddProperty(name, value, options);
        }
        bool caseSensitive = !(options?.Options?.PropertyNameCaseInsensitive ?? true);
        TraversePredicate predicate =
                    CreatePathPredicate(path, caseSensitive);

        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            source.Filter(writer, predicate, OnTryAdd);
        }
        var reader = new Utf8JsonReader(bufferWriter.WrittenSpan);
        var result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;

        JsonElement? OnTryAdd(JsonElement target, IImmutableList<string> breadcrumbs)
        {
            var result = target.TryAddProperty(name, value, options);
            return result;
        }

    }

    /// <summary>
    /// Try add property into a json object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source json.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    /// <exception cref="System.NotSupportedException">Only object are eligible as target for TryAddProperty</exception>
    public static JsonElement TryAddProperty<T>(
                                this in JsonElement source,
                                string name,
                                T value,
                                JsonPropertyModificatonOpions? options = null)
    {
        if (source.ValueKind != JsonValueKind.Object)
            throw new NotSupportedException("Only object are eligible as target for TryAddProperty");

        options = options ?? new JsonPropertyModificatonOpions();

        bool ignoreCase = options.Options?.PropertyNameCaseInsensitive ?? true;
        bool ignoreNull = options.IgnoreNull;
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            bool exists = false;
            foreach (JsonProperty item in source.EnumerateObject())
            {
                bool shouldOverride = false;
                if (string.Compare(item.Name, name, ignoreCase) == 0)
                {
                    exists = true;
                    shouldOverride = item.Value.ValueKind switch
                    {
                        JsonValueKind.Null => ignoreNull,
                        JsonValueKind.Undefined => ignoreNull,
                        _ => false
                    };
                }
                if (shouldOverride)
                    WriteProp(item.Name);
                else
                    item.WriteTo(writer);
            }

            if (!exists)
                WriteProp(name);

            writer.WriteEndObject();

            void WriteProp(string propName)
            {
                writer.WritePropertyName(propName);
                var val = value.ToJson(options.Options);
                val.WriteTo(writer);
            }
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan);
        JsonDocument result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;
    }

    #endregion // TryAddProperty
}
