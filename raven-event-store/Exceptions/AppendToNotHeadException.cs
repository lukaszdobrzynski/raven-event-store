using System;

namespace Raven.EventStore.Exceptions;

public class AppendToNotHeadException(string message) : Exception(message);