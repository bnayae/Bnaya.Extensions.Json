namespace System.Text.Json;

static partial class JsonExtensions
{
    /// <summary>
    /// Rewrite json while excluding elements which doesn't match the path
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
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
