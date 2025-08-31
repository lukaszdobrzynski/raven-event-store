using System;
using System.Collections.Generic;

namespace Raven.EventStore.Tests.Asserts;

public static class GlobalLogAssert
{
    public static void LogCount(List<GlobalEventLog> logs, int expected)
    {
        Assert.That(logs, Has.Count.EqualTo(expected));
    }
    
    public static void StreamId(GlobalEventLog log, string expected)
    {
        Assert.That(log.StreamId, Is.EqualTo(expected));
    }

    public static void StreamKey(GlobalEventLog log, Guid expected)
    {
        Assert.That(log.StreamKey, Is.EqualTo(expected));
    }
    
    public static void EventId(GlobalEventLog log, Guid expected)
    {
        Assert.That(log.Event.EventId, Is.EqualTo(expected));
    }
}