using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentEventStoreException(string name) : Exception($"The Event Store with the name {name} does not exist.");