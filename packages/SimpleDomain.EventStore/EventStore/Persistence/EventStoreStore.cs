namespace SimpleDomain.EventStore.Persistence;

using System.Diagnostics.CodeAnalysis;
using global::EventStore.Client;

[ExcludeFromCodeCoverage]
internal class EventStoreStore(EventStoreClientFactory factory, TypeNameStrategy typeNameStrategy) : IEventStore
{
    public Task<IEventStream> OpenStream<TAggregateRoot>(string aggregateId, CancellationToken cancellationToken)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        Task.FromResult<IEventStream>(
            new EventStoreStream<TAggregateRoot>(aggregateId, factory.Create(), typeNameStrategy)
        );

    public async Task ReplayAll(Func<object, CancellationToken, Task> publishEvent, CancellationToken cancellationToken)
    {
        await using var client = factory.Create();
        var events = client.ReadAllAsync(Direction.Forwards, Position.Start, cancellationToken: cancellationToken);
        await foreach (var e in events)
        {
            if (e.Event.EventType.StartsWith("$", StringComparison.InvariantCultureIgnoreCase)) continue;
            if (e.Event.EventType.Contains("Snapshot", StringComparison.InvariantCultureIgnoreCase)) continue;

            await publishEvent(e.DeserializeAsEvent(), cancellationToken);
        }
    }
}