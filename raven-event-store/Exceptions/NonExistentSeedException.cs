using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentSeedException : Exception
{
    public NonExistentSeedException(string streamId, string seedId)
        : base($"Stream {streamId} references seed {seedId} but the seed document does not exist.") { }

    public NonExistentSeedException(string streamId, string seedId, string detail)
        : base($"Stream {streamId} references seed {seedId}: {detail}.") { }
}
