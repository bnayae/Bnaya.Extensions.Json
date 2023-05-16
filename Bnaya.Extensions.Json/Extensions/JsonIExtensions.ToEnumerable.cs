using System.Collections.Generic;
using System.Collections.Immutable;

using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseMarkSemantic;

// credit: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to

namespace System.Text.Json;

static partial class JsonExtensions
{
    #region Overloads

    /// <summary>
    /// Filters descendant element by path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static IEnumerable<JsonElement> ToEnumerable(
        this JsonDocument source,
        string path)
    {
        return source.RootElement.ToEnumerable(path);
    }

    /// <summary>
    /// Filters descendant element by path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static IEnumerable<JsonElement> ToEnumerable(
        this in JsonElement source,
        string path)
    {
        return source.ToEnumerable(false, path);
    }

    /// <summary>
    /// Filters descendant element by path.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="caseSensitive">indicate whether path should be a case sensitive</param>
    /// <param name="path">The path.</param>
    /// <param name="semantic">
    /// The Semantic of marking a node
    /// </param>
    /// <returns></returns>
    public static IEnumerable<JsonElement> ToEnumerable(
        this in JsonElement source,
        bool caseSensitive,
        string path,
        TraverseMarkSemantic semantic = TraverseMarkSemantic.Pick)
    {
        TraversePredicate predicate =
            CreatePathPredicate(path, caseSensitive);
        return source.ToEnumerableRec(ImmutableList<string>.Empty, predicate, semantic);
    }


    /// <summary>
    /// Filters descendant element by predicate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="predicate">
    /// <![CDATA[The predicate: (current, deep, breadcrumbs spine) => ...;
    /// current: the current JsonElement.
    /// deep: start at 0.
    /// breadcrumbs spine: spine of ancestor's properties and arrays index.
    /// TIP: using static System.Text.Json.TraverseFlowInstruction;]]>
    /// </param>
    /// <param name="semantic">
    /// The Semantic of marking a node
    /// </param>
    /// <returns></returns>
    public static IEnumerable<JsonElement> ToEnumerable(
                            this JsonDocument source,
                            TraversePredicate predicate,
                            TraverseMarkSemantic semantic = TraverseMarkSemantic.Pick)
    {
        return source.RootElement.ToEnumerableRec(ImmutableList<string>.Empty, predicate, semantic);
    }

    /// <summary>
    /// Filters descendant element by predicate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="predicate">
    /// <![CDATA[The predicate: (current, deep, breadcrumbs spine) => ...;
    /// current: the current JsonElement.
    /// deep: start at 0.
    /// breadcrumbs spine: spine of ancestor's properties and arrays index.
    /// TIP: using static System.Text.Json.TraverseFlowInstruction;]]>
    /// </param>
    /// <param name="semantic">
    /// The Semantic of marking a node
    /// </param>
    /// <returns></returns>
    public static IEnumerable<JsonElement> ToEnumerable(
        this in JsonElement source,
        TraversePredicate predicate,
        TraverseMarkSemantic semantic = TraverseMarkSemantic.Pick)
    {
        return source.ToEnumerableRec(ImmutableList<string>.Empty, predicate, semantic);
    }

    #endregion // Overloads

    /// <summary>
    /// Filters descendant element by predicate.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="spine">The breadcrumbs spine.</param>
    /// <param name="predicate"><![CDATA[The predicate: (current, deep, breadcrumbs spine) => ...;
    /// current: the current JsonElement.
    /// deep: start at 0.
    /// breadcrumbs spine: spine of ancestor's properties and arrays index.
    /// TIP: using static System.Text.Json.TraverseFlowInstruction;]]></param>
    /// <param name="semantic">The Semantic of marking a node</param>
    /// <returns></returns>
    private static IEnumerable<JsonElement> ToEnumerableRec(
                            this JsonElement source,
                            IImmutableList<string> spine,
                            TraversePredicate predicate,
                            TraverseMarkSemantic semantic = TraverseMarkSemantic.Pick)
    {
        if (source.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty p in source.EnumerateObject())
            {
                var spn = spine.Add(p.Name);
                var val = p.Value;

                var (flow, marked) = predicate(val, spn);
                if (marked && semantic == Pick || !marked && semantic == Ignore)
                {
                    yield return val;
                }
                if (flow == Sibling)
                    continue;
                if (flow == Parent)
                    break;
                if (flow == Children)
                {
                    foreach (var result in val.ToEnumerableRec(spn, predicate))
                    {
                        yield return result;
                    }
                }
                else if (flow == Stop)
                    yield break;
            }
        }
        else if (source.ValueKind == JsonValueKind.Array)
        {
            int i = 0;
            foreach (JsonElement val in source.EnumerateArray())
            {
                var spn = spine.Add($"[{i++}]");
                var (flow, marked) = predicate(val, spn);
                if (marked)
                {
                    yield return val;
                }

                if (flow == Sibling)
                    continue;
                if (flow == Parent)
                    break;
                if (flow == Children)
                {
                    foreach (var result in val.ToEnumerableRec(spn, predicate))
                    {
                        yield return result;
                    }
                }
                else if (flow == Stop)
                    yield break;
            }
        }

    }

}
