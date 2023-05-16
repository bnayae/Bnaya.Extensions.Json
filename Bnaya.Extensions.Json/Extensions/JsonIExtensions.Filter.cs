﻿using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Disposables;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Bnaya.Extensions.Common.Disposables;
using Bnaya.Extensions.Json.Commands;
using static Bnaya.Extensions.Json.Commands.FilterCommands;

using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseMarkSemantic;

namespace System.Text.Json;

// TODO: Match all property/array items, if none, don't Start/Array/Object
// TODO: inheritance + pattern matching JsonProperty?JsonElement -> predicate
// TODO: ^ bake into the breadcrumb 
// TODO: recursion should return whether it wrote a value, if not, under property it should write a null 

static partial class JsonExtensions
{
    #region enum RecResults

    [Flags]
    enum RecResults
    {
        None = 0,
        //Null = 1,
        Stop = 2
    }

    #endregion // enum RecResults

    #region Overloads

    /// <summary>
    /// Rewrite json while excluding elements which doesn't match the predicate
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="predicate"><![CDATA[The predicate: (current, deep, breadcrumbs spine) => ...;
    /// current: the current JsonElement.
    /// deep: start at 0.
    /// breadcrumbs spine: spine of ancestor's properties and arrays index.
    /// TIP: using static System.Text.Json.TraverseFlowControl;]]></param>
    /// <param name="onMatch"><![CDATA[
    /// Notify when find a match.
    /// According to the return value it will replace or remove the element.
    /// Replaced when returning alternative `JsonElement` otherwise Removed.
    /// Action's signature : (current, deep, breadcrumbs) => ...;]]></param>
    /// <returns></returns>
    public static JsonElement Filter(
        this JsonDocument source,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null)
    {
        return source.RootElement.Filter(predicate, onMatch);
    }

    /// <summary>
    /// Rewrite json while excluding elements which doesn't match the predicate
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="predicate"><![CDATA[The predicate: (current, deep, breadcrumbs spine) => ...;
    /// current: the current JsonElement.
    /// deep: start at 0.
    /// breadcrumbs spine: spine of ancestor's properties and arrays index.
    /// TIP: using static System.Text.Json.TraverseFlowControl;]]></param>
    /// <param name="onMatch"><![CDATA[
    /// Notify when find a match.
    /// According to the return value it will replace or remove the element.
    /// Replaced when returning alternative `JsonElement` otherwise Removed.
    /// Action's signature : (current, deep, breadcrumbs) => ...;]]></param>
    /// <returns></returns>
    public static JsonElement Filter(
        this in JsonElement source,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            source.Filter(writer, predicate, onMatch);
        }
        var reader = new Utf8JsonReader(bufferWriter.WrittenSpan);
        var result = JsonDocument.ParseValue(ref reader);
        return result.RootElement;
    }

    /// <summary>
    /// Filters the specified element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="onMatch">Optional replacement hook.</param>
    /// <returns></returns>
    private static void Filter(
        this in JsonElement element,
        Utf8JsonWriter writer,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null)
    {
        var breadcrumbs = Disposable.CreateCollection<string>();
        FilterRec(element, writer, breadcrumbs, predicate, onMatch);
    }

    #endregion // Overloads

    /// <summary>
    /// Filters the record.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="breadcrumbs">The breadcrumbs.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="onMatch">The on replace.</param>
    /// <param name="writeCommand">The write command (execute on full match [mark]).</param>
    /// <returns></returns>
    private static RecResults FilterRec(
        this in JsonElement element,
        Utf8JsonWriter writer,
        CollectionDisposable<string> breadcrumbs,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null,
        IWriteCommand? writeCommand = null)
    {
        IWriteCommand cmd = writeCommand ?? FilterCommands.Non;

        if (element.ValueKind == JsonValueKind.Object)
        {
            using (var cmd1 = CreateObjectWriter(writer, breadcrumbs.State, cmd))
            {
                var elements = element.EnumerateObject();
                foreach (var property in elements)
                {
                    string propName = property.Name;
                    using (var spine = breadcrumbs.Add(propName))
                    using (var cmd2 = CreatePropertyWriter(propName, writer, spine.State, cmd1))
                    {
                        var val = property.Value;
                        var (flow, mark) = predicate(val, spine.State);

                        if (mark)
                        {
                            using (var cmd3 = CreateElementWriter(val, onMatch, writer, breadcrumbs.State, cmd2))
                            {
                                cmd3.Write();
                            }
                        }
                        else if (flow == Children)
                        {
                            RecResults response = val.FilterRec(writer, breadcrumbs, predicate, onMatch, cmd2);
                            //if ((response & RecResults.Null) != RecResults.None)
                            //    writer.WriteNullValue();
                            if ((response & RecResults.Stop) != RecResults.None)
                                return RecResults.Stop;
                        }
                        if (flow == Sibling)
                            continue;
                        if (flow == Parent)
                            break;
                        if (flow == Stop)
                            return RecResults.Stop;
                    }

                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            using (var cmd1 = CreateArryWriter(writer, breadcrumbs.State, cmd))
            {
                int i = 0;
                var elements = element.EnumerateArray();
                foreach (var val in elements)
                {
                    using (var spine = breadcrumbs.Add($"[{i++}]"))
                    {
                        var (flow, mark) = predicate(val, spine.State);
                        if (mark)
                        {
                            using (var cmd2 = CreateElementWriter(val, onMatch, writer, breadcrumbs.State, cmd1))
                            {
                                cmd2.Write();
                            }
                        }
                        else if (flow == Children)
                        {
                            RecResults response = val.FilterRec(writer, breadcrumbs, predicate, onMatch, cmd1);
                            //if ((response & RecResults.Null) != RecResults.None)
                            //    writer.WriteNullValue();
                            if ((response & RecResults.Stop) != RecResults.None)
                                return RecResults.Stop;
                        }
                        if (flow == Sibling)
                            continue;
                        if (flow == Parent)
                            break;
                        if (flow == Stop)
                            return RecResults.Stop;
                    }
                }
            }
        }
        else
        {
            var (flow, mark) = predicate(element, breadcrumbs.State);
            if (mark)
            {
                using (var cmd1 = CreateElementWriter(element, onMatch, writer, breadcrumbs.State, cmd))
                {
                    cmd1.Write();
                }
            }
            if (flow == Stop)
                return RecResults.Stop;
        }
        return RecResults.None;
    }
}
