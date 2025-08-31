using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentEventStoreException(string databaseName) : Exception($"The Event Store with the database name {databaseName} does not exist.");