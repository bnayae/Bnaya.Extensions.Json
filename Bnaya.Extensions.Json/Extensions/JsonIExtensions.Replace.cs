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
    /// Rewrite json while excluding elements according to a path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="onReplace">The on replace.</param>
    /// <param name="caseSensitive">indicate whether path should be a case sensitive</param>
    /// <returns></returns>
    public static JsonElement Replace(
        this JsonDocument source,
        string path,
        JsonMatchHook onReplace,
        bool caseSensitive = false)
    {
        return source.RootElement.Replace(path, onReplace, caseSensitive);
    }

    /// <summary>
    /// Rewrite json while excluding elements according to a path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="onReplace">The replacement strategy.</param>
    /// <param name="caseSensitive">indicate whether path should be a case sensitive</param>
    /// <returns></returns>
    public static JsonElement Replace(
        this in JsonElement source,
        string path,
        JsonMatchHook onReplace,
        bool caseSensitive = false)
    {
        TraversePredicate predicate =
            CreatePathPredicate(path, caseSensitive, TraverseMarkSemantic.Replace);
        return source.Filter(predicate, onReplace);
    }
}
