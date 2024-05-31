namespace SimpleDomain.EventStore.Persistence;

using FakeItEasy;
using TestDoubles;

public class InMemoryIntegrationTests
{
    private const string EventDescriptors = InMemoryEventStore.EventDescriptors;
    private const string SnapshotDescriptors = InMemoryEventStore.SnapshotDescriptors;
    private const string AggregateId = "aggregate-id";
    private readonly List<EventDescriptor> eventDescriptors = [];
    private readonly IHaveEventStoreConfiguration eventStoreConfiguration = A.Fake<IHaveEventStoreConfiguration>();
    private readonly List<SnapshotDescriptor> snapshotDescriptors = [];

    public InMemoryIntegrationTests()
    {
        A.CallTo(() => this.eventStoreConfiguration.Get<List<EventDescriptor>>(EventDescriptors))
            .Returns(this.eventDescriptors);
        A.CallTo(() => this.eventStoreConfiguration.Get<List<SnapshotDescriptor>>(SnapshotDescriptors))
            .Returns(this.snapshotDescriptors);
    }

    [Fact]
    public async Task AppendsAndReplaysAllEvents()
    {
        var aggregateRoot = new MyDynamicEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(0);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        using var eventStream = await this.CreateEventStream();
        await eventStream.Open(default);
        await eventStream.Append(
            aggregateRoot.UncommittedEvents.OfType<VersionableEvent>().ToList(),
            aggregateRoot.Version,
            new Dictionary<string, object>(),
            default);

        var eventHistory = await eventStream.Replay(default);
        eventHistory.Should().HaveCount(3);
        eventHistory.Should().Contain(e => (e as ValueEvent)!.Value == 0);
        eventHistory.Should().Contain(e => (e as ValueEvent)!.Value == 11);
        eventHistory.Should().Contain(e => (e as ValueEvent)!.Value == 22);
    }

    [Fact]
    public async Task AppendsAndReplaysEventsFromLatestSnapshot()
    {
        var aggregateRoot = new MyDynamicEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(0);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        await this.AppendEvents(aggregateRoot);

        var firstSnapshot = aggregateRoot.CreateSnapshot();
        await this.SaveSnapshot(firstSnapshot);

        aggregateRoot.ChangeValue(33);
        aggregateRoot.ChangeValue(44);

        await this.AppendEvents(aggregateRoot);

        var secondSnapshot = aggregateRoot.CreateSnapshot();
        await this.SaveSnapshot(secondSnapshot);

        aggregateRoot.ChangeValue(55);
        aggregateRoot.ChangeValue(66);
        aggregateRoot.ChangeValue(77);

        await this.AppendEvents(aggregateRoot);

        using var eventStream = await this.CreateEventStream();
        var hasSnapshot = await eventStream.HasSnapshot(default);
        var snapshotFromEventStream = await eventStream.GetLatestSnapshot(default);

        hasSnapshot.Should().BeTrue();
        snapshotFromEventStream.Should().BeEquivalentTo(secondSnapshot);

        var eventHistorySinceLatestSnapshot = await eventStream.ReplayFromSnapshot(snapshotFromEventStream, default);
        eventHistorySinceLatestSnapshot.Should().HaveCount(3);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as ValueEvent)!.Value == 55);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as ValueEvent)!.Value == 66);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as ValueEvent)!.Value == 77);
    }

    [Fact]
    public async Task ReplaysAllEventsInOrderOfOccurrence()
    {
        var dynamicAggregateRoot = new MyDynamicEventSourcedAggregateRoot("dynamic-1");
        var staticAggregateRoot = new MyStaticEventSourcedAggregateRoot("static-1");

        dynamicAggregateRoot.ChangeValue(11);
        dynamicAggregateRoot.ChangeValue(22);
        staticAggregateRoot.ChangeValue(111);
        dynamicAggregateRoot.ChangeValue(33);
        staticAggregateRoot.ChangeValue(222);
        staticAggregateRoot.ChangeValue(333);

        await this.AppendEvents(dynamicAggregateRoot);
        await this.AppendEvents(staticAggregateRoot);

        var eventPublisher = A.Fake<Func<object, CancellationToken, Task>>();
        var eventStore = new InMemoryEventStore(this.eventStoreConfiguration);
        await eventStore.ReplayAll(eventPublisher, default);

        A.CallTo(() => eventPublisher.Invoke(A<object>._, A<CancellationToken>._))
            .MustHaveHappened(6, Times.Exactly);

        A.CallTo(() => eventPublisher.Invoke(
                A<ValueEvent>.That.Matches(v => v.Value == 11), A<CancellationToken>._)).MustHaveHappened()
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<ValueEvent>.That.Matches(v => v.Value == 22), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<ValueEvent>.That.Matches(v => v.Value == 111), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<ValueEvent>.That.Matches(v => v.Value == 33), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<ValueEvent>.That.Matches(v => v.Value == 222), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<ValueEvent>.That.Matches(v => v.Value == 333), A<CancellationToken>._)).MustHaveHappened());
    }

    private Task<IEventStream> CreateEventStream()
    {
        var eventStore = new InMemoryEventStore(this.eventStoreConfiguration);
        return eventStore.OpenStream<MyDynamicEventSourcedAggregateRoot>(AggregateId, default);
    }

    private async Task AppendEvents(EventSourcedAggregateRoot aggregateRoot)
    {
        using var eventStream = await this.CreateEventStream();
        await eventStream.Append(
            aggregateRoot.UncommittedEvents.OfType<VersionableEvent>().ToList(),
            aggregateRoot.Version,
            new Dictionary<string, object>(),
            default);
        aggregateRoot.CommitEvents();
    }

    private async Task SaveSnapshot(ISnapshot snapshot)
    {
        using var eventStream = await this.CreateEventStream();
        await eventStream.SaveSnapshot(snapshot, default);
    }
}
