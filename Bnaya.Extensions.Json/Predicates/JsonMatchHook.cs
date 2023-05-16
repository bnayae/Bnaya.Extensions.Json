using System.Collections.Immutable;

// credit: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to


namespace System.Text.Json;

/// <summary>
/// Use to notify on matches and potentially replace a json element with an alternative
/// </summary>
/// <param name="element">The element.</param>
/// <param name="breadcrumbs">The breadcrumbs.</param>
/// <returns>The replacement if any or null to ignore it</returns>
public delegate JsonElement? JsonMatchHook(
                            JsonElement element,
                            IImmutableList<string> breadcrumbs);
