using static System.Text.Json.TraverseMarkSemantic;

namespace System.Text.Json;

static partial class JsonExtensions
{
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
}
