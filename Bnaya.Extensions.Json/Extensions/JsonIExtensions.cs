﻿using System.Buffers;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;

using static System.Text.Json.Extension.Constants;

// credit: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to


namespace System.Text.Json;


/// <summary>
/// Json extensions
/// </summary>
public static partial class JsonExtensions
{
    private static JsonWriterOptions INDENTED_JSON_OPTIONS = new JsonWriterOptions { Indented = true };

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

        var result = source.Replace(path, OnTryAdd, caseSensitive);
        return result;

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
