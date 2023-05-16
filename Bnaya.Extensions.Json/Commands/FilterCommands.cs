using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Disposables;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using Bnaya.Extensions.Common.Disposables;
using Bnaya.Extensions.Json.deprecated;

using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseMarkSemantic;
#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace Bnaya.Extensions.Json.Commands;

/// <summary>
/// Filter commands
/// </summary>
internal static class FilterCommands
{
    /// <summary>
    /// A non write command
    /// </summary>
    public static readonly IWriteCommand Non = new NonWriteCommand();

    #region Create... [factories]

    /// <summary>
    /// Creates an arry writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="breadcrumbs">The breadcrumbs.</param>
    /// <param name="parent">The parent.</param>
    /// <returns></returns>
    public static IWriteCommand CreateArryWriter(
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) => new WriteArrayCommand(writer, breadcrumbs, parent);

    /// <summary>
    /// Creates an object writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="breadcrumbs">The breadcrumbs.</param>
    /// <param name="parent">The parent.</param>
    /// <returns></returns>
    public static IWriteCommand CreateObjectWriter(
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) => new WriteObjectCommand(writer, breadcrumbs, parent);

    /// <summary>
    /// Creates a property writer.
    /// </summary>
    /// <param name="propName">Name of the property.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="breadcrumbs">The breadcrumbs.</param>
    /// <param name="parent">The parent.</param>
    /// <returns></returns>
    public static IWriteCommand CreatePropertyWriter(
                        string propName,
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) => new WritePropertyCommand(propName, writer, breadcrumbs, parent);

    /// <summary>
    /// Creates an element writer.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="onMatch">The on match.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="breadcrumbs">The breadcrumbs.</param>
    /// <param name="parent">The parent.</param>
    /// <returns></returns>
    public static IWriteCommand CreateElementWriter(
                        JsonElement element,
                        JsonMatchHook? onMatch,
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) => new WriteElementCommand(element, onMatch, writer, breadcrumbs, parent);

    #endregion // Create... [factories]

    #region NonWriteCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="writer"
    /// <param name="element"></param>
    /// <param name="breadcrumbs"></param>
    /// <param name="onMatch"></param>
    /// <param name="propName"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    public record NonWriteCommand() : IWriteCommand
    {
        #region IEnumerator Members

        public IEnumerator<IWriteCommand> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion // IEnumerator Members

        #region Write

        /// <summary>
        /// Write the element
        /// </summary>
        void IWriteCommand.Write()
        {
        }

        #endregion // Write

        #region Dispose

        /// <summary>
        /// Close ending phrase like arrays or objects.
        /// </summary>
        void IDisposable.Dispose()
        {
        }

        #endregion // Dispose
    }

    #endregion // NonWriteCommand

    #region WriteCommandAbstract

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="writer"
    /// <param name="element"></param>
    /// <param name="breadcrumbs"></param>
    /// <param name="onMatch"></param>
    /// <param name="propName"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    [DebuggerDisplay("{breadcrumbs}")]
    public abstract record WriteCommandAbstract(
        Utf8JsonWriter writer,
        IImmutableList<string> breadcrumbs,
        IWriteCommand? parent) : IWriteCommand
    {
        public bool executed;

        #region IEnumerator Members

        public IEnumerator<IWriteCommand> GetEnumerator()
        {
            yield return this;
            if (parent == null)
                yield break;
            foreach (var item in parent)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion // IEnumerator Members

        #region Write

        /// <summary>
        /// Write the element
        /// </summary>
        void IWriteCommand.Write()
        {
            if (executed)
                return;
            executed = true;

            parent?.Write();
            OnWrite();
        }

        #endregion // Write

        #region OnWrite

        /// <summary>
        /// Called when should write.
        /// </summary>
        /// <returns>
        /// </returns>
        protected abstract void OnWrite();

        #endregion // OnWrite

        #region Dispose

        /// <summary>
        /// Close ending phrase like arrays or objects.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (executed)
                OnClose();
        }

        #endregion // Dispose

        #region OnClose

        /// <summary>
        /// Called when should write.
        /// </summary>
        /// <returns>
        /// </returns>
        protected virtual void OnClose()
        {
        }

        #endregion // OnClose
    }

    #endregion // WriteCommandAbstract

    #region WriteArrayCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="writer"
    /// <param name="breadcrumbs"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    public record WriteArrayCommand(
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) : WriteCommandAbstract(writer, breadcrumbs, parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnWrite() => writer.WriteStartArray();

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        protected override void OnClose() => writer.WriteEndArray();
    }

    #endregion // WriteArrayCommand

    #region WriteObjectCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="writer"
    /// <param name="breadcrumbs"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    public record WriteObjectCommand(
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) : WriteCommandAbstract(writer, breadcrumbs, parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnWrite() => writer.WriteStartObject();

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        protected override void OnClose() => writer.WriteEndObject();
    }

    #endregion // WriteObjectCommand

    #region WritePropertyCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="writer"
    /// <param name="element"></param>
    /// <param name="breadcrumbs"></param>
    /// <param name="onMatch"></param>
    /// <param name="propName"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    [DebuggerDisplay("{propName}, {breadcrumbs}")]
    public record WritePropertyCommand(
                        string propName,
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) : WriteCommandAbstract(writer, breadcrumbs, parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnWrite() => writer.WritePropertyName(propName);
    }

    #endregion // WritePropertyCommand

    #region WriteElementCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="writer"
    /// <param name="element"></param>
    /// <param name="breadcrumbs"></param>
    /// <param name="onMatch"></param>
    /// <param name="propName"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    [DebuggerDisplay("{element}, {breadcrumbs}")]
    public record WriteElementCommand(
                        JsonElement element,
                        JsonMatchHook? onMatch,
                        Utf8JsonWriter writer,
                        IImmutableList<string> breadcrumbs,
                        IWriteCommand? parent) : WriteCommandAbstract(writer, breadcrumbs, parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnWrite()
        {
            var replacement = onMatch?.Invoke(element, breadcrumbs) ?? element;
            replacement.WriteTo(writer);
        }
    }

    #endregion // WriteElementCommand
}
