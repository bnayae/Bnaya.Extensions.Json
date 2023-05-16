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
/// Use to notify on matches and potentially replace a json element with an alternative
/// </summary>
/// <param name="element">The element.</param>
/// <param name="breadcrumbs">The breadcrumbs.</param>
/// <returns>The replacement if any or null to ignore it</returns>
public delegate JsonElement? JsonMatchHook(
                            JsonElement element,
                            IImmutableList<string> breadcrumbs);
