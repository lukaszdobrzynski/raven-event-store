using System;

namespace Raven.EventStore.Exceptions;

public class EmptyStreamIdException() : Exception("Stream ID cannot be empty.");