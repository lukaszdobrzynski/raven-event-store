using System;

namespace Raven.EventStore.Exceptions;

internal class NonExistentEventStoreException(string name) : Exception($"The Event Store with the name {name} does not exist.");