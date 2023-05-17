namespace System.Text.Json;

/// <summary>
/// Predicate result's action
/// </summary>
public enum TraverseAction
{
    /// <summary>
    /// Do nothing
    /// </summary>
    None,
    /// <summary>
    /// Follow the onMatch delegate instruction if exists, otherwise take the element.
    /// </summary>
    TakeOrReplace,
    /// <summary>
    /// Take the element, ignore replacement logic.
    /// Used by the replace logic.
    /// </summary>
    Take,
}
