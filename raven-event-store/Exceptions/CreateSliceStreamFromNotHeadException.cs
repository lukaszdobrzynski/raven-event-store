using System;

namespace Raven.EventStore.Exceptions;

public class SliceStreamNotHeadException (string message) : Exception(message);
