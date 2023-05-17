using System.Collections.Immutable;

// credit: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to


namespace System.Text.Json;

/// <summary>
/// Use to mark element of a json for traversing
/// </summary>
/// <param name="element">The element.</param>
/// <param name="breadcrumbs">The breadcrumbs.</param>
/// <returns>Instruction for the next step of traversing</returns>
public delegate TraverseInstruction TraversePredicate(
                            JsonElement element,
                            IImmutableList<string> breadcrumbs);
