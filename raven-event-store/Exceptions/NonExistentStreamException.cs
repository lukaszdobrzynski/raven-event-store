using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentStreamException : Exception
{
    public NonExistentStreamException(string streamId)
        : base($"The stream with the ID {streamId} does not exist.") { }

    public NonExistentStreamException(Guid streamKey)
        : base($"The stream chain with the key {streamKey} does not exist.") { }
}