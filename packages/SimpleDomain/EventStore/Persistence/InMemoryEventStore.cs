namespace SimpleDomain.EventStore.Persistence;

internal class InMemoryEventStore(IHaveEventStoreConfiguration configuration) : IEventStore
{
    public const string EventDescriptors = "EventDescriptors";
    public const string SnapshotDescriptors = "SnapshotDescriptors";

    public Task<IEventStream> OpenStream<TAggregateRoot>(string aggregateId, CancellationToken cancellationToken)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        var eventStream = new InMemoryEventStream<TAggregateRoot>(
            aggregateId,
            configuration.Get<List<EventDescriptor>>(EventDescriptors),
            configuration.Get<List<SnapshotDescriptor>>(SnapshotDescriptors));

        return eventStream.Open(cancellationToken);
    }

    public async Task ReplayAll(
        Func<object, CancellationToken, Task> publishEvent,
        CancellationToken cancellationToken)
    {
        var eventDescriptors = configuration.Get<List<EventDescriptor>>(EventDescriptors);
        foreach (var eventDescriptor in eventDescriptors.OrderBy(e => e.Timestamp))
            await publishEvent(eventDescriptor.Event, cancellationToken);
    }
}