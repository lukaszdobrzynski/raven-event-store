using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentAggregateException(string streamId, string aggregateId)
    : Exception($"Stream {streamId} references aggregate {aggregateId} but the aggregate document does not exist.");
