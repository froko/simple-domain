namespace SimpleDomain.EventStore;

/// <summary>
/// The EventStore repository.
/// </summary>
/// <param name="eventStore">Dependency injection for <see cref="IEventStore" />.</param>
/// <param name="configuration">Dependency injection for <see cref="IHaveEventStoreConfiguration" />.</param>
public class EventStoreRepository(IEventStore eventStore, IHaveEventStoreConfiguration configuration)
    : IEventSourcedRepository
{
    /// <inheritdoc />
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

            var events = await eventStream.ReplayFromSnapshot(snapshot, cancellationToken);
            await aggregateRoot.LoadFromEventHistory(events);
        }
        else
        {
            var events = await eventStream.Replay(cancellationToken);
            await aggregateRoot.LoadFromEventHistory(events);
        }

        return aggregateRoot;
    }

    /// <inheritdoc />
    public Task Save<TAggregateRoot>(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.Save(aggregateRoot, new Dictionary<string, object>(), cancellationToken);

    /// <inheritdoc />
    public Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.Save(aggregateRoot, headers, _ => Task.CompletedTask, cancellationToken);

    /// <inheritdoc />
    public Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        Func<object, Task> publishEvent,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.Save(aggregateRoot, new Dictionary<string, object>(), publishEvent, cancellationToken);

    /// <inheritdoc />
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
        where TAggregateRoot : EventSourcedAggregateRoot
        => eventStore.OpenStream<TAggregateRoot>(aggregateId, cancellationToken);

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