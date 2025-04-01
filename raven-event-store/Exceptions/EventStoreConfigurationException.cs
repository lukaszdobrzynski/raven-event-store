using System;

namespace Raven.EventStore.Exceptions;

internal class EventStoreConfigurationException : Exception
{
    public EventStoreConfigurationException(string message) : base(message)
    {
    }

    public EventStoreConfigurationException(string message, Exception exception) : base(message, exception)
    {
    }
}