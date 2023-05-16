#pragma warning disable RCS1093 // Remove file with no code.

//using System.Buffers;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.IO;
//using System.Linq;
//using System.Numerics;

//using Bnaya.Extensions.Json.deprecated;

//using static System.Text.Json.Extension.Constants;


//using static System.Text.Json.TraverseFlowControl;
//using static System.Text.Json.TraverseFlow;
//using static System.Text.Json.TraverseMarkSemantic;

//namespace System.Text.Json.Advance;


///// <summary>
///// Json extensions
///// </summary>
//static partial class JsonExtensions
//{
//    /// <summary>
//    /// Merge source json with other json at specific location within the source
//    /// Note: 
//    /// - On conflicts, will override the source
//    /// - Array will be concatenate.
//    /// </summary>
//    /// <param name="source">The source.</param>
//    /// <param name="predicate">Identify the merge with element.</param>
//    /// <param name="joined">The joined element (will override on conflicts).</param>
//    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
//    /// <returns></returns>
//    public static JsonElement MergeInto(
//        this JsonDocument source,
//        Func<JsonElement, IImmutableList<string>, TraverseFlowControl> predicate,
//        params JsonElement[] joined)
//    {
//        return source.RootElement.MergeInto(predicate, (IEnumerable<JsonElement>)joined);
//    }

//    /// <summary>
//    /// Merge source json with other json at specific location within the source
//    /// Note: 
//    /// - On conflicts, will override the source
//    /// - Array will be concatenate.
//    /// </summary>
//    /// <param name="source">The source.</param>
//    /// <param name="predicate">Identify the merge with element.</param>
//    /// <param name="joined">The joined element (will override on conflicts).</param>
//    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
//    /// <returns></returns>
//    public static JsonElement MergeInto(
//        this in JsonElement source,
//        Func<JsonElement, IImmutableList<string>, TraverseFlowControl> predicate,
//        params JsonElement[] joined)
//    {
//        return source.MergeInto(predicate, (IEnumerable<JsonElement>)joined);
//    }

//    /// <summary>
//    /// Merge source json with other json at specific location within the source
//    /// Note: 
//    /// - On conflicts, will override the source
//    /// - Array will be concatenate.
//    /// </summary>
//    /// <param name="source">The source.</param>
//    /// <param name="predicate">Identify the merge with element.</param>
//    /// <param name="joined">The joined element (will override on conflicts).</param>
//    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
//    /// <returns></returns>
//    public static JsonElement MergeInto(
//        this JsonDocument source,
//        Func<JsonElement, IImmutableList<string>, TraverseFlowControl> predicate,
//        IEnumerable<JsonElement> joined)
//    {
//        return source.RootElement.MergeInto(predicate, joined);
//    }

//    /// <summary>
//    /// Merge source json with other json at specific location within the source
//    /// Note: 
//    /// - On conflicts, will override the source
//    /// - Array will be concatenate.
//    /// </summary>
//    /// <param name="source">The source.</param>
//    /// <param name="predicate">Identify the merge with element.</param>
//    /// <param name="joined">The joined element (will override on conflicts).</param>
//    /// <remarks>Conflicts may happens when trying to merge object into array or other versa.</remarks>
//    /// <returns></returns>
//    public static JsonElement MergeInto(
//        this in JsonElement source,
//        Func<JsonElement, IImmutableList<string>, TraverseFlowControl> predicate,
//        IEnumerable<JsonElement> joined)
//    {
//        return source.Filter(predicate, OnMerge);

//        JsonElement? OnMerge(JsonElement target, IImmutableList<string> breadcrumbs)
//        {
//            return target.Merge(joined);
//        }
//    }
//}
