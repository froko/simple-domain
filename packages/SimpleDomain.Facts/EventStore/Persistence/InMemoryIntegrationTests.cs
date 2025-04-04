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
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(0);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        using var eventStream = await this.CreateEventStream<MyEventSourcedAggregateRoot>();
        await eventStream.Append(
            [.. aggregateRoot.UncommittedEvents.OfType<VersionableEvent>()],
            aggregateRoot.Version,
            new Dictionary<string, object>(),
            default);

        var asyncEventsFromStream = await eventStream.Replay(default);
        var events = await asyncEventsFromStream.ToListAsync();
        events.Should().HaveCount(3);
        events.Should().Contain(e => (e as MyEvent)!.Value == 0);
        events.Should().Contain(e => (e as MyEvent)!.Value == 11);
        events.Should().Contain(e => (e as MyEvent)!.Value == 22);
    }

    [Fact]
    public async Task AppendsAndReplaysEventsFromLatestSnapshot()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(0);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        await this.AppendEvents<MyEventSourcedAggregateRoot>(aggregateRoot);

        var firstSnapshot = aggregateRoot.CreateSnapshot();
        await this.SaveSnapshot<MyEventSourcedAggregateRoot>(firstSnapshot);

        aggregateRoot.ChangeValue(33);
        aggregateRoot.ChangeValue(44);

        await this.AppendEvents<MyEventSourcedAggregateRoot>(aggregateRoot);

        var secondSnapshot = aggregateRoot.CreateSnapshot();
        await this.SaveSnapshot<MyEventSourcedAggregateRoot>(secondSnapshot);

        aggregateRoot.ChangeValue(55);
        aggregateRoot.ChangeValue(66);
        aggregateRoot.ChangeValue(77);

        await this.AppendEvents<MyEventSourcedAggregateRoot>(aggregateRoot);

        using var eventStream = await this.CreateEventStream<MyEventSourcedAggregateRoot>();
        var hasSnapshot = await eventStream.HasSnapshot(default);
        var snapshotFromEventStream = await eventStream.GetLatestSnapshot(default);

        hasSnapshot.Should().BeTrue();
        snapshotFromEventStream.Should().BeEquivalentTo(secondSnapshot);

        var asyncEventsFromStream = await eventStream.ReplayFromSnapshot(snapshotFromEventStream, default);
        var eventHistorySinceLatestSnapshot = await asyncEventsFromStream.ToListAsync();
        eventHistorySinceLatestSnapshot.Should().HaveCount(3);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as MyEvent)!.Value == 55);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as MyEvent)!.Value == 66);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as MyEvent)!.Value == 77);
    }

    [Fact]
    public async Task ReplaysAllEventsInOrderOfOccurrence()
    {
        var myEventSourcedAggregateRoot = new MyEventSourcedAggregateRoot("my-1");
        var otherEventSourceAggregateRoot = new OtherEventSourceAggregateRoot("other-1");

        myEventSourcedAggregateRoot.ChangeValue(11);
        await Task.Delay(1);
        myEventSourcedAggregateRoot.ChangeValue(22);
        await Task.Delay(1);
        otherEventSourceAggregateRoot.ChangeValue(111);
        await Task.Delay(1);
        myEventSourcedAggregateRoot.ChangeValue(33);
        await Task.Delay(1);
        otherEventSourceAggregateRoot.ChangeValue(222);
        await Task.Delay(1);
        otherEventSourceAggregateRoot.ChangeValue(333);

        await this.AppendEvents<MyEventSourcedAggregateRoot>(myEventSourcedAggregateRoot);
        await this.AppendEvents<OtherEventSourceAggregateRoot>(otherEventSourceAggregateRoot);

        var eventPublisher = A.Fake<Func<object, CancellationToken, Task>>();
        var eventStore = new InMemoryEventStore(this.eventStoreConfiguration);
        await eventStore.ReplayAll(eventPublisher, default);

        A.CallTo(() => eventPublisher.Invoke(A<object>._, A<CancellationToken>._))
            .MustHaveHappened(6, Times.Exactly);

        A.CallTo(() => eventPublisher.Invoke(
                A<MyEvent>.That.Matches(v => v.Value == 11), A<CancellationToken>._)).MustHaveHappened()
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<MyEvent>.That.Matches(v => v.Value == 22), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<OtherEvent>.That.Matches(v => v.Value == 111), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<MyEvent>.That.Matches(v => v.Value == 33), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<OtherEvent>.That.Matches(v => v.Value == 222), A<CancellationToken>._)).MustHaveHappened())
            .Then(A.CallTo(() => eventPublisher.Invoke(
                A<OtherEvent>.That.Matches(v => v.Value == 333), A<CancellationToken>._)).MustHaveHappened());
    }

    private Task<IEventStream> CreateEventStream<TAggregateRoot>()
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        var eventStore = new InMemoryEventStore(this.eventStoreConfiguration);
        return eventStore.OpenStream<TAggregateRoot>(AggregateId, default);
    }

    private async Task AppendEvents<TAggregateRoot>(EventSourcedAggregateRoot aggregateRoot)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        using var eventStream = await this.CreateEventStream<TAggregateRoot>();
        await eventStream.Append(
            [.. aggregateRoot.UncommittedEvents.OfType<VersionableEvent>()],
            aggregateRoot.Version,
            new Dictionary<string, object>(),
            default);
        aggregateRoot.CommitEvents();
    }

    private async Task SaveSnapshot<TAggregateRoot>(ISnapshot snapshot)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        using var eventStream = await this.CreateEventStream<TAggregateRoot>();
        await eventStream.SaveSnapshot(snapshot, default);
    }
}