namespace System.Text.Json;

/// <summary>
/// Traversing flow instruction
/// </summary>
/// <param name="Next">
/// Gets or sets the flow direction of the json's traversing.
/// </param>
/// <param name="Marked">
/// Mark the current json node.
/// Depending on the context is can be marked for peek or remove. 
/// </param>
public record struct TraverseInstruction(
    TraverseFlow Next = TraverseFlow.Children,
    TraverseAction Marked = TraverseAction.None)
{
    /// <summary>
    /// Traversing to its children.
    /// Means that it might be part of a path for a nested marked element.
    /// </summary>
    public static readonly TraverseInstruction ToChildren = new TraverseInstruction(TraverseFlow.Children);
    /// <summary>
    /// Ignore the current node and continue traversing to its sibling (without marking the current node)
    /// </summary>
    public static readonly TraverseInstruction SkipToSibling = new TraverseInstruction(TraverseFlow.Sibling);
    /// <summary>
    /// Mark the current node and continue traversing to its sibling (skip sibling)
    /// </summary>
    public static readonly TraverseInstruction TakeOrReplace = new TraverseInstruction(TraverseFlow.Sibling, TraverseAction.TakeOrReplace);
    /// <summary>
    /// Mark the current node and continue traversing to its sibling (skip sibling)
    /// </summary>
    public static readonly TraverseInstruction Take = new TraverseInstruction(TraverseFlow.Sibling, TraverseAction.Take);
    /// <summary>
    /// Ignore the current node and continue traversing to its parent (without marking the current node)
    /// </summary>
    public static readonly TraverseInstruction SkipToParent = new TraverseInstruction(TraverseFlow.Parent);

    /// <summary>
    /// Performs an implicit conversion />.
    /// </summary>
    /// <param name="next">The next.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator TraverseInstruction(TraverseFlow next) => new TraverseInstruction(next);
}
