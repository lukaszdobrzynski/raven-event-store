using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task AppendAndStoreAsync(Guid streamKey, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        await AppendAndStoreAsync(streamKey, events?.ToList(), useOptimisticConcurrency: false, cancellationToken);
    }

    public void AppendAndStore(Guid streamKey, IEnumerable<Event> events)
    {
        AppendAndStore(streamKey, events?.ToList(), useOptimisticConcurrency: false);
    }

    public async Task AppendAndStoreAsyncOptimistically(Guid streamKey, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        await AppendAndStoreAsync(streamKey, events?.ToList(), useOptimisticConcurrency: true, cancellationToken);
    }

    public void AppendAndStoreOptimistically(Guid streamKey, IEnumerable<Event> events)
    {
        AppendAndStore(streamKey, events?.ToList(), useOptimisticConcurrency: true);
    }

    public async Task AppendAndStoreAsync(Guid streamKey, params Event[] events)
    {
        await AppendAndStoreAsync(streamKey, events?.ToList(), useOptimisticConcurrency: false, CancellationToken.None);
    }

    public void AppendAndStore(Guid streamKey, params Event[] events)
    {
        AppendAndStore(streamKey, events?.ToList(), useOptimisticConcurrency: false);
    }

    public async Task AppendAndStoreAsyncOptimistically(Guid streamKey, params Event[] events)
    {
        await AppendAndStoreAsync(streamKey, events?.ToList(), useOptimisticConcurrency: true, CancellationToken.None);
    }

    public void AppendAndStoreOptimistically(Guid streamKey, params Event[] events)
    {
        AppendAndStore(streamKey, events?.ToList(), useOptimisticConcurrency: true);
    }

    private async Task AppendAndStoreAsync(Guid streamKey, List<Event> events, bool useOptimisticConcurrency, CancellationToken cancellationToken = default)
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            await HandleAppendAsync(session, streamKey, events, cancellationToken);
            await session.SaveChangesAsync(cancellationToken);
        }
    }

    private void AppendAndStore(Guid streamKey, List<Event> events, bool useOptimisticConcurrency)
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = OpenSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            HandleAppend(session, streamKey, events);
            session.SaveChanges();
        }
    }
}