using System;

namespace Raven.EventStore;

public class HeadStreamPointer
{
    public string Id { get; internal set; }
    public string HeadStreamId { get; internal set; }

    internal static HeadStreamPointer Create(Guid streamKey, string headStreamId) => new()
    {
        Id = GetId(streamKey),
        HeadStreamId = headStreamId
    };

    internal static string GetId(Guid streamKey) => $"HeadPointers/{streamKey}";
}
