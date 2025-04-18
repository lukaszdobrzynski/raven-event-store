namespace Raven.EventStore.Tests.Asserts;

public static class EventAssert
{
    public static void Version(Event @event, int expected)
    {
        Assert.That(@event.Version, Is.EqualTo(expected)); 
    }

    public static void Type<T>(Event @event) where T : Event
    {
        Assert.That(@event, Is.InstanceOf<T>());
    }
}