using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public record FromFile
    {
        public required string Name { get; init; }
        public required string Role { get; init; }
        public required int Stars { get; init; }
        public required ConsoleColor Color { get; init; }
        public required ImmutableList<string> Tags { get; init; }
        public /* required */ ImmutableDictionary<string, ConsoleColor> Headers { get; init; } = ImmutableDictionary<string, ConsoleColor>.Empty;
        [JsonPropertyName("kvs")]
        public /* required */ ImmutableDictionary<string, KeyValuePair<string, ConsoleColor>> Map { get; init; } = ImmutableDictionary<string, KeyValuePair<string, ConsoleColor>>.Empty;
    }
}
