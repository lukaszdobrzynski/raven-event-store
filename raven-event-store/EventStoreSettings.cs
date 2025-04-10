using System;
using System.Collections.Generic;

namespace Raven.EventStore;

internal class EventStoreSettings
{
    public readonly HashSet<Type> Snapshots = [];
    public bool UseGlobalStreamLogging { get; set; }
}