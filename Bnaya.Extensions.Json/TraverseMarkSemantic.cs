﻿namespace System.Text.Json;

/// <summary>
/// The Semantic of marking a node in the JSON's traversing flow.
/// </summary>
public enum TraverseMarkSemantic
{
    /// <summary>
    /// The pick the marked elements
    /// </summary>
    Pick,
    /// <summary>
    /// The ignore branches under a marked element unless it part of other unmarked branches 
    /// </summary>
    Ignore
}
