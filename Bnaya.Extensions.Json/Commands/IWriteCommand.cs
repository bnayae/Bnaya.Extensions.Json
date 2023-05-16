using System;
using System.Collections.Generic;

namespace Bnaya.Extensions.Json.Commands;

/// <summary>
/// Contract for the write command.
/// The action (write) will be execute at most once.
/// The disposable won't be execute unless the action invoked.
/// </summary>
public interface IWriteCommand : IEnumerable<IWriteCommand>, IDisposable
{
    /// <summary>
    /// Writes the specified semantic.
    /// </summary>
    void Run();

    /// <summary>
    /// Gets the dept.
    /// </summary>
    int Deep { get; }
}
