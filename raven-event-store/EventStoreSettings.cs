using System;
using System.Collections.Generic;

namespace Raven.EventStore;

internal class EventStoreSettings
{
    public readonly HashSet<Type> Aggregates = [];
    public bool UseGlobalStreamLogging { get; set; }
    public string DatabaseName { get; set; }
}