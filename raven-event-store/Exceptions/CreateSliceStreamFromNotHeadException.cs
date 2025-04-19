using System;

namespace Raven.EventStore.Exceptions;

public class CreateSliceStreamFromNotHeadException (string message) : Exception(message);
