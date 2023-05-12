namespace System.Text.Json;

/// <summary>
/// Traversing flow control
/// </summary>
/// <param name="Next">
/// Gets or sets the flow direction of the json's traversing.
/// </param>
/// <param name="Marked">
/// Mark the current json node.
/// Depending on the context is can be marked for peek or remove. 
/// </param>
public record struct TraverseFlowControl(
    TraverseFlow Next = TraverseFlow.Children,
    bool Marked = false)
{
    /// <summary>
    /// The default state
    /// </summary>
    public static readonly TraverseFlowControl Default = SkipToChildren;
    /// <summary>
    /// Ignore the current node and continue traversing to its children (without marking the current node)
    /// </summary>
    public static readonly TraverseFlowControl SkipToChildren = new TraverseFlowControl(TraverseFlow.Children);
    /// <summary>
    /// Mark the current node and continue traversing to its children (skip sibling)
    /// </summary>
    public static readonly TraverseFlowControl MarkToChildren = new TraverseFlowControl(TraverseFlow.Children, true);
    /// <summary>
    /// Ignore the current node and continue traversing to its sibling (without marking the current node)
    /// </summary>
    public static readonly TraverseFlowControl SkipToSibling = new TraverseFlowControl(TraverseFlow.Sibling);
    /// <summary>
    /// Mark the current node and continue traversing to its sibling (skip sibling)
    /// </summary>
    public static readonly TraverseFlowControl MarkToSibling = new TraverseFlowControl(TraverseFlow.Sibling, true);
    /// <summary>
    /// Ignore the current node and continue traversing to its parent (without marking the current node)
    /// </summary>
    public static readonly TraverseFlowControl SkipToParent = new TraverseFlowControl(TraverseFlow.Parent);
    /// <summary>
    /// Mark the current node and continue traversing to its parent (skip sibling)
    /// </summary>
    public static readonly TraverseFlowControl MarkToParent = new TraverseFlowControl(TraverseFlow.Parent, true);

    /// <summary>
    /// Performs an implicit conversion from <see cref="Bnaya.Extensions.Json.TraverceNext"/> to <see cref="Bnaya.Extensions.Json.TraverceFlowControl"/>.
    /// </summary>
    /// <param name="next">The next.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator TraverseFlowControl(TraverseFlow next) => new TraverseFlowControl(next);
}
