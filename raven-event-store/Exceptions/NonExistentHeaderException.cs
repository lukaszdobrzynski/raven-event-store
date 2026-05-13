using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentHeaderException : Exception
{
    public NonExistentHeaderException(Guid streamKey)
        : base($"The header for the stream with key {streamKey} does not exist.") { }
}
