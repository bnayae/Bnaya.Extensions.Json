using System;
using System.Collections.Generic;

namespace Bnaya.Extensions.Json.Commands;

/// <summary>
/// Contract for the write command.
/// The action (write) will be execute at most once.
/// The disposable won't be execute unless the action invoked.
/// </summary>
/// <seealso cref="IEnumerable&lt;JsonExtensions.IWriteCommand&gt;" />
/// <seealso cref="IDisposable" />
public interface IWriteCommand : IEnumerable<IWriteCommand>, IDisposable
    {
        void Write();
    }
