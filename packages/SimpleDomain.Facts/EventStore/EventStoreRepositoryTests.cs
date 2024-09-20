namespace SimpleDomain.EventStore;

using FakeItEasy;
using TestDoubles;

public class EventStoreRepositoryTests
{
    private const string AggregateId = "Test-1234";
    private readonly IHaveEventStoreConfiguration configuration = A.Fake<IHaveEventStoreConfiguration>();

    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IEventStream eventStream = A.Fake<IEventStream>();
    private readonly EventStoreRepository testee;

    public EventStoreRepositoryTests()
    {
        A.CallTo(() => this.eventStore
                .OpenStream<MyEventSourcedAggregateRoot>(AggregateId, default))
            .Returns(this.eventStream);

        A.CallTo(() => this.configuration
                .GetSnapshotStrategy<MyEventSourcedAggregateRoot>())
            .Returns(new SnapshotStrategy(100));

        this.testee = new EventStoreRepository(this.eventStore, this.configuration);
    }

    [Fact]
    public async Task ReturnsAggregateRootByIdByReplayingAllEvents()
    {
        var events = new[] { new MyEvent(0), new MyEvent(11), new MyEvent(22) };
        A.CallTo(() => this.eventStream.Replay(default)).Returns(events.ToAsyncEnumerable());

        var aggregateRoot = await this.testee
            .GetById<MyEventSourcedAggregateRoot>(AggregateId);

        aggregateRoot.Version.Should().Be(2);
        aggregateRoot.Value.Should().Be(22);
    }

    [Fact]
    public async Task? ReturnsAggregateRootByIdByReplayingAllEventsFromSnapshot()
    {
        var snapshot = new MySnapshot(22, 2, DateTimeOffset.Now);
        var eventsSinceSnapshot = new[] { new MyEvent(33), new MyEvent(44) };

        A.CallTo(() => this.eventStream.HasSnapshot(default)).Returns(true);
        A.CallTo(() => this.eventStream.GetLatestSnapshot(default)).Returns(snapshot);
        A.CallTo(() => this.eventStream.ReplayFromSnapshot(snapshot, default))
            .Returns(eventsSinceSnapshot.ToAsyncEnumerable());

        var aggregateRoot = await this.testee
            .GetById<MyEventSourcedAggregateRoot>(AggregateId);

        aggregateRoot.Version.Should().Be(4);
        aggregateRoot.Value.Should().Be(44);
    }

    [Fact]
    public async Task SavesEventsWithoutHeaders()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        await this.testee.Save(aggregateRoot);

        A.CallTo(() => this.eventStream
                .Append(
                    A<IReadOnlyCollection<VersionableEvent>>._,
                    aggregateRoot.Version,
                    A<IDictionary<string, object>>._,
                    default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SavesEventsWithHeaders()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        var headers = new Dictionary<string, object> { { "UserName", "Patrick" }, { "MagicNumber", 42 } };

        await this.testee.Save(aggregateRoot, headers);

        A.CallTo(() => this.eventStream
            .Append(
                A<IReadOnlyCollection<VersionableEvent>>._,
                aggregateRoot.Version, headers, default)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SavesEventsWithoutHeadersAndEventPublicationCallback()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        await this.testee.Save(aggregateRoot);

        A.CallTo(() => this.eventStream
                .Append(
                    A<IReadOnlyCollection<VersionableEvent>>._,
                    aggregateRoot.Version,
                    A<IDictionary<string, object>>._,
                    default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SavesEventsWithHeadersAndEventPublicationCallback()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        var headers = new Dictionary<string, object> { { "UserName", "Patrick" }, { "MagicNumber", 42 } };

        await this.testee.Save(aggregateRoot, headers);

        A.CallTo(() => this.eventStream
                .Append(
                    A<IReadOnlyCollection<VersionableEvent>>._,
                    aggregateRoot.Version,
                    headers,
                    default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CommitsUncommittedEventsOnAggregateRootAfterSavingUncommittedEvents()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        await this.testee.Save(aggregateRoot, _ => Task.CompletedTask);

        aggregateRoot.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SavesWithSnapshot()
    {
        A.CallTo(() => this.configuration.GetSnapshotStrategy<MyEventSourcedAggregateRoot>())
            .Returns(new SnapshotStrategy(10));

        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId).WithVersion(10);

        await this.testee.Save(aggregateRoot, _ => Task.CompletedTask);

        A.CallTo(() => this.eventStream.SaveSnapshot(A<ISnapshot>._, default)).MustHaveHappened();
    }
}