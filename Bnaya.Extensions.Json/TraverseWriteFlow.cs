namespace System.Text.Json;

/// <summary>
/// Json's flow traversing direction.
/// </summary>
public enum TraverseWriteFlow
{
    /// <summary>
    /// Drill into child nodes, than proceed to sibling and finally back to parent
    /// </summary>
    Children,
    /// <summary>
    /// Proceed to the next sibling and finally back to parent (skip children)
    /// </summary>
    Sibling,
    /// <summary>
    /// Go back to parent (skip children and sibling)
    /// </summary>
    Parent,
    /// <summary>
    /// Exit the traverse flow
    /// </summary>
    Stop
}
