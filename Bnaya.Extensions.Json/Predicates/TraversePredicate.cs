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
/// Use to mark element of a json for traversing
/// </summary>
/// <param name="element">The element.</param>
/// <param name="breadcrumbs">The breadcrumbs.</param>
/// <returns>Instruction for the next step of traversing</returns>
public delegate TraverseInstruction TraversePredicate(
                            JsonElement element,
                            IImmutableList<string> breadcrumbs);
