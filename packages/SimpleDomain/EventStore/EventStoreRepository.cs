namespace SimpleDomain.EventStore;

internal class EventStoreRepository(IEventStore eventStore, IHaveEventStoreConfiguration configuration)
    : IEventSourcedRepository
{
    public async Task<TAggregateRoot> GetById<TAggregateRoot>(
        string aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        var aggregateRoot = Activator.CreateInstance<TAggregateRoot>();

        using var eventStream = await this.OpenStream<TAggregateRoot>(aggregateId, cancellationToken);
        if (await eventStream.HasSnapshot(cancellationToken))
        {
            var snapshot = await eventStream.GetLatestSnapshot(cancellationToken);
            aggregateRoot.LoadFromSnapshot(snapshot);

            var eventHistory = await eventStream.ReplayFromSnapshot(snapshot, cancellationToken);
            aggregateRoot.LoadFromEventHistory(eventHistory);
        }
        else
        {
            var eventHistory = await eventStream.Replay(cancellationToken);
            if (eventHistory.IsEmpty) throw new AggregateRootNotFoundException(typeof(TAggregateRoot), aggregateId);

            aggregateRoot.LoadFromEventHistory(eventHistory);
        }

        return aggregateRoot;
    }

    public Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        Func<object, Task> publishEvent,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.Save(aggregateRoot, new Dictionary<string, object>(), publishEvent, cancellationToken);

    public async Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        IDictionary<string, object> headers,
        Func<object, Task> publishEvent,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        using var eventStream = await this.OpenStream<TAggregateRoot>(aggregateRoot.Id, cancellationToken);

        var uncommittedEvents = aggregateRoot.UncommittedEvents.OfType<VersionableEvent>().ToList();
        await eventStream.Append(uncommittedEvents, aggregateRoot.Version, headers, cancellationToken);
        uncommittedEvents.Select(e => e.InnerEvent).ToList().ForEach(e => publishEvent(e));
        aggregateRoot.CommitEvents();

        await this.SaveSnapshotIfRequired(aggregateRoot, cancellationToken);
    }

    private Task<IEventStream> OpenStream<TAggregateRoot>(string aggregateId, CancellationToken cancellationToken)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        eventStore.OpenStream<TAggregateRoot>(aggregateId, cancellationToken);

    private Task SaveSnapshotIfRequired<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        configuration.GetSnapshotStrategy<TAggregateRoot>().NeedsSnapshot(aggregateRoot)
            ? this.SaveSnapshot<TAggregateRoot>(
                aggregateRoot.Id,
                aggregateRoot.CreateSnapshot(),
                cancellationToken)
            : Task.CompletedTask;

    private async Task SaveSnapshot<TAggregateRoot>(
        string aggregateId,
        ISnapshot snapshot,
        CancellationToken cancellationToken)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        using var eventStream = await this.OpenStream<TAggregateRoot>(aggregateId, cancellationToken);
        await eventStream.SaveSnapshot(snapshot, cancellationToken);
    }
}
