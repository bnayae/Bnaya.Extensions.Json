using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public record FromFileInterface
    {
        public required string Name { get; init; }
        public required string Role { get; init; }
        public required int Stars { get; init; }
        public required ConsoleColor Color { get; init; }
        public required IImmutableList<string> Tags { get; init; }
        public /* required */ IImmutableDictionary<string, ConsoleColor> Headers { get; init; } = ImmutableDictionary<string, ConsoleColor>.Empty;
        [JsonPropertyName("kvs")]
        public /* required */ IImmutableDictionary<string, KeyValuePair<string, ConsoleColor>> Map { get; init; } = ImmutableDictionary<string, KeyValuePair<string, ConsoleColor>>.Empty;
    }
}
