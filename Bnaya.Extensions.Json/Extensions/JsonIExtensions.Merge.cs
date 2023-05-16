using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;

using Bnaya.Extensions.Json.deprecated;

using static System.Text.Json.Extension.Constants;


using static System.Text.Json.TraverseInstruction;
using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseMarkSemantic;

namespace System.Text.Json;


static partial class JsonExtensions
{
    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <returns></returns>
    public static JsonElement Merge(
        this JsonDocument source,
        params JsonElement[] joined) => source.RootElement.Merge(joined);

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <returns></returns>
    public static JsonElement Merge(
        this in JsonElement source,
        params JsonElement[] joined)
    {
        return source.Merge((IEnumerable<JsonElement>)joined);
    }

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <returns></returns>
    public static JsonElement Merge(
        this JsonDocument source,
        IEnumerable<JsonElement> joined)
    {
        return source.RootElement.Merge(joined);
    }

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <returns></returns>
    public static JsonElement Merge(
        this in JsonElement source,
        IEnumerable<JsonElement> joined)
    {
        return joined.Aggregate(source, (acc, cur) => acc.MergeImp(cur));
    }

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <param name="options">Serialization options</param>
    /// <returns></returns>
    public static JsonElement Merge<T>(
        this JsonDocument source,
        T joined,
        JsonSerializerOptions? options = null)
    {
        var j = joined.ToJson(options);
        return source.RootElement.MergeImp(j);
    }

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <param name="options">Serialization options</param>
    /// <returns></returns>
    public static JsonElement Merge<T>(
        this in JsonElement source,
        T joined,
        JsonSerializerOptions? options = null)
    {
        var j = joined.ToJson(options);
        return source.MergeImp(j);
    }

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <returns></returns>
    private static JsonElement MergeImp(
        this in JsonElement source,
        JsonElement joined)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            source.MergeImp(joined, writer);
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan);
        JsonDocument result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;
    }

    /// <summary>
    /// Merge source json with other json (which will override the source on conflicts)
    /// Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <param name="writer">The writer.</param>
    private static void MergeImp(
        this in JsonElement source,
        JsonElement joined,
        Utf8JsonWriter writer)
    {
        #region Validation

        if (source.ValueKind == JsonValueKind.Array && joined.ValueKind != JsonValueKind.Array)
        {
            joined.WriteTo(writer); // override
            return;
        }
        if (source.ValueKind == JsonValueKind.Object && joined.ValueKind != JsonValueKind.Object)
        {
            joined.WriteTo(writer); // override
            return;
        }
        if (joined.ValueKind != JsonValueKind.Object && joined.ValueKind != JsonValueKind.Array)
        {
            joined.WriteTo(writer); // override
            return;
        }

        #endregion // Validation

        if (source.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            var map = joined.EnumerateObject().ToDictionary(m => m.Name, m => m.Value);
            foreach (JsonProperty p in source.EnumerateObject())
            {

                var name = p.Name;
                var val = p.Value;

                writer.WritePropertyName(p.Name);
                if (map.ContainsKey(name))
                {
                    var j = map[name];
                    val.MergeImp(j, writer);
                    map.Remove(name);
                    continue;
                }
                val.WriteTo(writer);
            }
            foreach (var p in map)
            {
                var name = p.Key;
                writer.WritePropertyName(name);
                var val = p.Value;
                val.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
        else if (source.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (JsonElement val in source.EnumerateArray())
            {
                val.WriteTo(writer);
            }
            foreach (JsonElement val in joined.EnumerateArray())
            {
                val.WriteTo(writer);
            }
            writer.WriteEndArray();
        }
        else
        {
            joined.WriteTo(writer);
        }
    }
}
