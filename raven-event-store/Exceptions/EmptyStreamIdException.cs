using System;

namespace Raven.EventStore.Exceptions;

internal class EmptyStreamIdException() : Exception("Stream ID cannot be empty.");