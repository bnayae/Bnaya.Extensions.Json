using System.Collections.Immutable;

namespace System.Text.Json.Extension.Extensions.Tests
{
    /// <summary>
    /// Response structure
    /// </summary>
    public record KeyValueStyle
    {
        /// <summary>
        /// Gets he chapter selection.
        /// </summary>
        public ImmutableDictionary<string, string[]> KV { get; init; } = ImmutableDictionary<string, string[]>.Empty;
    }
}
