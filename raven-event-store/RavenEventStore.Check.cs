using System;
using System.Collections.Generic;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private static void CheckForNullEvents(List<Event> events)
    {
        if (events is null) 
            throw new ArgumentException("Events cannot be null.");

        var nullIndex = events.FindIndex(e => e is null);
        
        if (nullIndex != -1)
            throw new ArgumentException($" Null event found at index {nullIndex}.");
    }

    private static void CheckForNonExistentStream<TStream>(TStream stream, string streamId) where TStream : DocumentStream
    {
        if (stream is null)
            throw new NonExistentStreamException(streamId);
    }

    private static void CheckForAttemptToCreateSliceStreamFromNonHead<TStream>(TStream stream)
        where TStream : DocumentStream
    {
        if (stream.IsHeadSlice == false)
            throw new CreateSliceStreamFromNotHeadException($"Cannot create a split stream from a non-head. " +
                                                  $"The stream with the ID {stream.Id} is a parent to an existing slice with the ID {stream.NextSliceId}.");
    }

    private static void CheckForAttemptToAppendToNonHead<TStream>(TStream stream) where TStream : DocumentStream
    {
        if (stream.IsHeadSlice == false)
            throw new AppendToNotHeadException($"Cannot append to a non-head. " +
                                               $"The stream with the ID {stream.Id} is a parent to an existing slice with the ID {stream.NextSliceId}.");
    }
}