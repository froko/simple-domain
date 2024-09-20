namespace SimpleDomain.EventStore.Persistence;

internal class InMemoryEventStream<TAggregateRoot> : EventStream<TAggregateRoot>
    where TAggregateRoot : EventSourcedAggregateRoot
{
    private readonly IList<EventDescriptor> eventDescriptors;
    private readonly IList<SnapshotDescriptor> snapshotDescriptors;

    internal InMemoryEventStream(
        string aggregateId,
        IList<EventDescriptor> eventDescriptors,
        IList<SnapshotDescriptor> snapshotDescriptors
    )
        : base(aggregateId)
    {
        this.eventDescriptors = eventDescriptors;
        this.snapshotDescriptors = snapshotDescriptors;
    }

    public override Task<IEventStream> Open(CancellationToken cancellationToken) => Task.FromResult<IEventStream>(this);

    public override Task SaveSnapshot(ISnapshot snapshot, CancellationToken cancellationToken)
    {
        this.snapshotDescriptors.Add(SnapshotDescriptor.From<TAggregateRoot>(this.AggregateId, snapshot));
        return Task.CompletedTask;
    }

    public override Task<bool> HasSnapshot(CancellationToken cancellationToken)
    {
        var hasSnapshot = this.snapshotDescriptors.Any(s =>
            s.AggregateType == this.AggregateType && s.AggregateId == this.AggregateId
        );
        return Task.FromResult(hasSnapshot);
    }

    public override Task<ISnapshot> GetLatestSnapshot(CancellationToken cancellationToken)
    {
        var latestSnapshot = this
            .snapshotDescriptors.Last(s => s.AggregateType == this.AggregateType && s.AggregateId == this.AggregateId)
            .Snapshot;
        return Task.FromResult(latestSnapshot);
    }

    protected override Task Save(
        VersionableEvent @event,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken
    )
    {
        this.eventDescriptors.Add(EventDescriptor.From<TAggregateRoot>(this.AggregateId, @event, headers));
        return Task.CompletedTask;
    }

    protected override async Task<IAsyncEnumerable<object>> Replay(int fromVersion, CancellationToken cancellationToken)
    {
        var events = await this.Replay(fromVersion);
        return events.ToAsyncEnumerable();
    }

    private Task<IEnumerable<object>> Replay(int fromVersion)
    {
        var events = this
            .eventDescriptors.Where(e => e.AggregateType == this.AggregateType && e.AggregateId == this.AggregateId)
            .Where(e => e.Version >= fromVersion)
            .OrderBy(e => e.Version)
            .Select(e => e.Event);

        return Task.FromResult(events);
    }
}