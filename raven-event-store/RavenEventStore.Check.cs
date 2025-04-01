using System;
using System.Collections.Generic;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private static void CheckForNullOrEmptyEvents(List<Event> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var nullIndex = events.FindIndex(e => e is null);
        
        if (nullIndex != -1)
            throw new ArgumentException($"Null event found at index {nullIndex}.");
    }

    private static void CheckForNonExistentStream<TStream>(TStream stream, string streamId) where TStream : DocumentStream
    {
        if (stream is null)
            throw new NonExistentStreamException(streamId);
    }
}