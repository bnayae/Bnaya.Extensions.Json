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
static partial class JsonExtensions
{
    #region CreatePathPredicate

    /// <summary>
    /// Creates a read's path predicate for filtering
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="semantic">The semantic.</param>
    /// <returns></returns>
    private static TraversePredicate CreatePathPredicate(
                                string path,
                                bool caseSensitive = false,
                                TraverseMarkSemantic semantic = TraverseMarkSemantic.Pick)
    {
        var filter = path.Split('.');

        TraverseInstruction Predicate(JsonElement current, IImmutableList<string> breadcrumbs)
        {
            int deep = breadcrumbs.Count - 1;
            var cur = breadcrumbs[deep];
            var validationPath = filter.Length > deep ? filter[deep] : "";
            bool objTerm = validationPath == "*" || string.Compare(validationPath, cur, !caseSensitive) == 0;
            bool arrTerm = validationPath == "[]" && cur[0] == '[' && cur[^1] == ']';
            if (objTerm || arrTerm)
            {
                if (semantic != TraverseMarkSemantic.Ignore)
                {
                    if (deep == filter.Length - 1)
                        return Mark;
                    return ToChildren;
                }


                if (deep == filter.Length - 1)
                    return SkipToSibling;
                return ToChildren;
            }

            if (semantic == TraverseMarkSemantic.Ignore)
            {
                if (deep == filter.Length - 1)
                    return Mark;
                return ToChildren;
            }
            return SkipToSibling;
        }
        return Predicate;
    }

    #endregion // CreatePathPredicate
}
