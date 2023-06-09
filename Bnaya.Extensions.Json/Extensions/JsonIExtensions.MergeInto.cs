﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace System.Text.Json;

static partial class JsonExtensions
{
    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="joined"></param>
    /// <param name="caseSensitive"></param>
    /// <param name="options">Serialization options</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto<T>(
        this in JsonElement source,
        string path,
        T joined,
        bool caseSensitive = false,
        JsonSerializerOptions? options = null)
    {
        var merged = joined.ToJson(options);

        return source.Replace(path, OnMerge);

        JsonElement? OnMerge(JsonElement target, IImmutableList<string> breadcrumbs)
        {
            return target.Merge(merged);
        }
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this JsonDocument source,
        string path,
        params JsonElement[] joined)
    {
        return source.RootElement.MergeInto(path, joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this in JsonElement source,
        string path,
        params JsonElement[] joined)
    {
        return source.MergeInto(path, false, (IEnumerable<JsonElement>)joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this JsonDocument source,
        string path,
        IEnumerable<JsonElement> joined)
    {
        return source.RootElement.MergeInto(path, joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this in JsonElement source,
        string path,
        IEnumerable<JsonElement> joined)
    {
        return source.MergeInto(path, false, joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this JsonDocument source,
        string path,
        bool caseSensitive,
        params JsonElement[] joined)
    {
        return source.RootElement.MergeInto(path, caseSensitive, joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this in JsonElement source,
        string path,
        bool caseSensitive,
        params JsonElement[] joined)
    {
        return source.MergeInto(path, caseSensitive, (IEnumerable<JsonElement>)joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this JsonDocument source,
        string path,
        bool caseSensitive,
        IEnumerable<JsonElement> joined)
    {
        return source.RootElement.MergeInto(path, caseSensitive, joined);
    }

    /// <summary>
    /// Merge source json with other json at specific location within the source
    /// Note: 
    /// - On conflicts, will override the source
    /// - Array will be concatenate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The target path for merging.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="joined">The joined element (will override on conflicts).</param>
    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
    /// <returns></returns>
    public static JsonElement MergeInto(
        this in JsonElement source,
        string path,
        bool caseSensitive,
        IEnumerable<JsonElement> joined)
    {
        return source.Replace(path, OnMerge, caseSensitive);

        JsonElement? OnMerge(JsonElement target, IImmutableList<string> breadcrumb)
        {
            return target.Merge(joined);
        }
    }
}
