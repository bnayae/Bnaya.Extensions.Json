using System.Buffers;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;

using static System.Text.Json.Extension.Constants;

using static System.Text.Json.TraverseMarkSemantic;

namespace System.Text.Json;

static partial class JsonExtensions
{
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
        bool caseSensitive = false)
    {
        return source.RootElement.Keep(path, caseSensitive);
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
        bool caseSensitive = false)
    {
        TraversePredicate predicate =
            CreatePathPredicate(path, caseSensitive);
        return source.Filter(predicate);
    }
}
