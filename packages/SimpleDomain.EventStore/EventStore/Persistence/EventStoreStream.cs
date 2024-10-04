namespace SimpleDomain.EventStore.Persistence;

using System.Diagnostics.CodeAnalysis;
using global::EventStore.Client;

[ExcludeFromCodeCoverage]
internal class EventStoreStream<TAggregateRoot>(
    string aggregateId,
    EventStoreClient client,
    TypeNameStrategy typeNameStrategy
) : EventStream<TAggregateRoot>(aggregateId)
    where TAggregateRoot : EventSourcedAggregateRoot
{
    private readonly string aggregateName = $"{typeof(TAggregateRoot).Name}-{aggregateId}";
    private readonly string snapshotName = $"{typeof(TAggregateRoot).Name}-{aggregateId}-Snapshot";

    public override Task<IEventStream> Open(CancellationToken cancellationToken) => Task.FromResult<IEventStream>(this);

    public override Task SaveSnapshot(ISnapshot snapshot, CancellationToken cancellationToken) =>
        client.AppendToStreamAsync(
            this.snapshotName,
            StreamState.Any,
            new[] { snapshot.SerializeSnapshot(typeNameStrategy) },
            cancellationToken: cancellationToken
        );

    public override async Task<bool> HasSnapshot(CancellationToken cancellationToken)
    {
        var stream = client.ReadStreamAsync(
            Direction.Backwards,
            this.snapshotName,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        return await stream.ReadState == ReadState.Ok;
    }

    public override async Task<ISnapshot> GetLatestSnapshot(CancellationToken cancellationToken)
    {
        var stream = client.ReadStreamAsync(
            Direction.Backwards,
            this.snapshotName,
            StreamPosition.End,
            cancellationToken: cancellationToken
        );
        var firstElement = await stream.FirstAsync(cancellationToken);
        return firstElement.DeserializeAsSnapshot();
    }

    protected override Task Save(
        VersionableEvent @event,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken
    ) =>
        client.AppendToStreamAsync(
            this.aggregateName,
            StreamState.Any,
            new[] { @event.SerializeInnerEvent(typeNameStrategy, headers) },
            cancellationToken: cancellationToken
        );

    protected override async Task<IAsyncEnumerable<object>> Replay(int fromVersion, CancellationToken cancellationToken)
    {
        var stream = client.ReadStreamAsync(
            Direction.Forwards,
            this.aggregateName,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        return await stream.ReadState == ReadState.StreamNotFound
            ? throw new AggregateRootNotFoundException(typeof(TAggregateRoot), this.AggregateId)
            : stream.DeserializeAsEvents(fromVersion);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            client.Dispose();
        base.Dispose(disposing);
    }
}