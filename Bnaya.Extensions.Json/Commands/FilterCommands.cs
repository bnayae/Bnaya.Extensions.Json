using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
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
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    public record NonWriteCommand() : IWriteCommand
    {
        #region Deep

        /// <summary>
        /// Gets the deep.
        /// </summary>
        public int Deep { get; } = -1;

        #endregion // Deep

        #region IEnumerator Members

        public IEnumerator<IWriteCommand> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion // IEnumerator Members

        #region Run

        /// <summary>
        /// Write the element
        /// </summary>
        void IWriteCommand.Run()
        {
        }

        #endregion // Run

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

    #region RunCommandAbstract

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    [DebuggerDisplay("{Breadcrumbs}")]
    public abstract record WriteCommandAbstract : IWriteCommand
    {
        public bool executed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterCommands" /> class.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="breadcrumbs">The breadcrumbs.</param>
        /// <param name="parent">The parent.</param>
        protected WriteCommandAbstract(
                                    Utf8JsonWriter writer,
                                    IImmutableList<string> breadcrumbs,
                                    IWriteCommand? parent)
        {
            Writer = writer;
            Breadcrumbs = breadcrumbs;
            Parent = parent;
            Deep = (parent?.Deep ?? 0) + 1;
        }

        #region Writer

        /// <summary>
        /// Gets the writer.
        /// </summary>
        public Utf8JsonWriter Writer { get; }

        #endregion // Writer

        #region Breadcrumbs

        /// <summary>
        /// Gets the breadcrumbs.
        /// </summary>
        public IImmutableList<string> Breadcrumbs { get; }

        #endregion // Breadcrumbs

        #region Parent

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public IWriteCommand? Parent { get; }

        #endregion // Parent

        #region Deep

        /// <summary>
        /// Gets the command's dept.
        /// </summary>
        public int Deep { get; private set; }

        #endregion // Deep

        #region IEnumerator Members

        public IEnumerator<IWriteCommand> GetEnumerator()
        {
            yield return this;
            if (Parent == null)
                yield break;
            foreach (var item in Parent)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion // IEnumerator Members

        #region Run

        /// <summary>
        /// Write the element
        /// </summary>
        public virtual void Run()
        {
            if (executed)
                return;
            executed = true;

            Parent?.Run();
            OnRun();
        }

        #endregion // Run

        #region OnRun

        /// <summary>
        /// Called when should write.
        /// </summary>
        /// <returns>
        /// </returns>
        protected abstract void OnRun();

        #endregion // OnRun

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

    #endregion // RunCommandAbstract

    #region RunArrayCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="Writer"></param>
    /// <param name="Breadcrumbs"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    /// <param name="Parent"></param>
    public record WriteArrayCommand(
                        Utf8JsonWriter Writer,
                        IImmutableList<string> Breadcrumbs,
                        IWriteCommand? Parent) : WriteCommandAbstract(Writer, Breadcrumbs, Parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnRun() => Writer.WriteStartArray();

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        protected override void OnClose() => Writer.WriteEndArray();
    }

    #endregion // RunArrayCommand

    #region RunObjectCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="Writer"></param>
    /// <param name="Breadcrumbs"></param>
    /// <param name="Parent"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    public record WriteObjectCommand(
                        Utf8JsonWriter Writer,
                        IImmutableList<string> Breadcrumbs,
                        IWriteCommand? Parent) : WriteCommandAbstract(Writer, Breadcrumbs, Parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnRun() => Writer.WriteStartObject();

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        protected override void OnClose() => Writer.WriteEndObject();
    }

    #endregion // RunObjectCommand

    #region RunPropertyCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="Writer"></param>
    /// <param name="Breadcrumbs"></param>
    /// <param name="PropName"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    /// <param name="Parent"></param>
    [DebuggerDisplay("{PropName}, {Breadcrumbs}")]
    public record WritePropertyCommand(
                        string PropName,
                        Utf8JsonWriter Writer,
                        IImmutableList<string> Breadcrumbs,
                        IWriteCommand? Parent) : WriteCommandAbstract(Writer, Breadcrumbs, Parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        protected override void OnRun() => Writer.WritePropertyName(PropName);
    }

    #endregion // RunPropertyCommand

    #region RunElementCommand

    /// <summary>
    /// Write command which will execute on final matching.
    /// </summary>
    /// <param name="Writer"></param>
    /// <param name="Element"></param>
    /// <param name="Breadcrumbs"></param>
    /// <param name="OnMatch"></param>
    /// <remarks>
    /// On complex match' like a pattern or path matching, 
    /// matching a parent element may be depend 
    /// on whether one of it's children has matched.
    /// </remarks>
    /// <param name="Parent"></param>
    [DebuggerDisplay("{Element}, {Breadcrumbs}")]
    public record WriteElementCommand(
                        JsonElement Element,
                        JsonMatchHook? OnMatch,
                        Utf8JsonWriter Writer,
                        IImmutableList<string> Breadcrumbs,
                        IWriteCommand? Parent) : WriteCommandAbstract(Writer, Breadcrumbs, Parent)
    {
        /// <summary>
        /// Write the element
        /// </summary>
        public override void Run()
        {
            if (OnMatch == null)
            {
                Parent?.Run(); // only if replaced
                Element.WriteTo(Writer);
            }
            else
            {
                var replacement = OnMatch.Invoke(Element, Breadcrumbs);
                if (replacement != null) 
                {
                    Parent?.Run(); // only if replaced
                    replacement?.WriteTo(Writer);
                }

                // null = remove the element
            }
        }

        /// <summary>
        /// Called when [run].
        /// </summary>
        protected override void OnRun()
        {
            throw new NotImplementedException();
        }
    }

    #endregion // RunElementCommand
}
