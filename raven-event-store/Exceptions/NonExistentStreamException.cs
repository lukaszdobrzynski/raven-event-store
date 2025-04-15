using System;

namespace Raven.EventStore.Exceptions;

public class NonExistentStreamException(string streamId) : Exception($"The stream with the ID {streamId} does not exist.");