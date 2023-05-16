using System.Buffers;
using System.Disposables;

using Bnaya.Extensions.Common.Disposables;
using Bnaya.Extensions.Json.Commands;

using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseMarkSemantic;
using static Bnaya.Extensions.Json.Commands.FilterCommands;

namespace System.Text.Json;

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
        return source.Filter(Pick, predicate, onMatch);
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
    /// <param name="semantic">The semantic.</param>
    /// <returns></returns>
    private static JsonElement Filter(
        this in JsonElement source,
        TraverseMarkSemantic semantic,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            source.Filter(writer, predicate, onMatch, semantic);
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
    /// <param name="semantic">The semantic.</param>
    /// <returns></returns>
    private static void Filter(
        this in JsonElement element,
        Utf8JsonWriter writer,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null,
        TraverseMarkSemantic semantic = TraverseMarkSemantic.Pick)
    {
        var breadcrumbs = Disposable.CreateCollection<string>();
        FilterRec(element, writer, breadcrumbs, predicate, onMatch, semantic);
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
    /// <param name="semantic">The semantic.</param>
    /// <param name="writeCommand">The write command (execute on full match [mark]).</param>
    /// <returns></returns>
    private static RecResults FilterRec(
        this in JsonElement element,
        Utf8JsonWriter writer,
        CollectionDisposable<string> breadcrumbs,
        TraversePredicate predicate,
        JsonMatchHook? onMatch = null,
        TraverseMarkSemantic semantic = Pick,
        IWriteCommand? writeCommand = null)
    {
        IWriteCommand cmd = writeCommand ?? FilterCommands.Non;

        if (element.ValueKind == JsonValueKind.Object)
        {
            using (var cmd1 = CreateObjectWriter(writer, breadcrumbs.State, cmd))
            {
                if(cmd1.Deep == 0)
                    cmd1.Run(); // root element

                var elements = element.EnumerateObject();
                foreach (var property in elements)
                {
                    string propName = property.Name;
                    using (var spine = breadcrumbs.Add(propName))
                    using (var cmd2 = CreatePropertyWriter(propName, writer, spine.State, cmd1))
                    {
                        var val = property.Value;
                        var (flow, mark) = predicate(val, spine.State);

                        if (mark && semantic == Pick || !mark && semantic == Ignore)
                        {
                            using (var cmd3 = CreateElementWriter(val, onMatch, writer, breadcrumbs.State, cmd2))
                            {
                                cmd3.Run();
                            }
                        }
                        else if (flow == Children)
                        {
                            RecResults response = val.FilterRec(writer, breadcrumbs, predicate, onMatch, semantic, cmd2);
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
                        if (mark && semantic == Pick || !mark && semantic == Ignore)
                        {
                            using (var cmd2 = CreateElementWriter(val, onMatch, writer, breadcrumbs.State, cmd1))
                            {
                                cmd2.Run();
                            }
                        }
                        else if (flow == Children)
                        {
                            RecResults response = val.FilterRec(writer, breadcrumbs, predicate, onMatch, semantic, cmd1);
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
            if (mark && semantic == Pick || !mark && semantic == Ignore)
            {
                using (var cmd1 = CreateElementWriter(element, onMatch, writer, breadcrumbs.State, cmd))
                {
                    cmd1.Run();
                }
            }

            if (flow == Stop)
                return RecResults.Stop;
        }
        return RecResults.None;
    }
}
