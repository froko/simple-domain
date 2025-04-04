namespace SimpleDomain.EventStore.Persistence;

using FakeItEasy;
using FluentAssertions;
using global::EventStore.Client;
using TestDoubles;
using Xunit;

public class EventStoreIntegrationTests : IAsyncLifetime
{
    private const string ConnectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";
    private const string AggregateId = "7890";

    private static string MyAggregateRootName => $"{nameof(MyEventSourcedAggregateRoot)}-{AggregateId}";
    private static string OtherAggregateRootName => $"{nameof(OtherEventSourceAggregateRoot)}-{AggregateId}";

    private static string OtherAggregateRootSnapshotName =>
        $"{nameof(OtherEventSourceAggregateRoot)}-{AggregateId}-Snapshot";

    public async Task InitializeAsync()
    {
        var eventStoreClientFactory = new EventStoreClientFactory(ConnectionString);
        await using var client = eventStoreClientFactory.Create();

        await DeleteEventStream(client, MyAggregateRootName);
        await DeleteEventStream(client, OtherAggregateRootName);
        await DeleteEventStream(client, OtherAggregateRootSnapshotName);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [RunnableInDebugOnly]
    public async Task AppendsAndReplaysAllEvents()
    {
        var aggregateRoot = new MyEventSourcedAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(0);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        using var eventStream = await CreateEventStream<MyEventSourcedAggregateRoot>();
        await eventStream.Append(
            [.. aggregateRoot.UncommittedEvents.OfType<VersionableEvent>()],
            aggregateRoot.Version,
            new Dictionary<string, object>(),
            default
        );

        var asyncEventsFromStream = await eventStream.Replay(default);
        var events = await asyncEventsFromStream.ToListAsync();
        events.Should().HaveCount(3);
        events.Should().Contain(e => (e as MyEvent)!.Value == 0);
        events.Should().Contain(e => (e as MyEvent)!.Value == 11);
        events.Should().Contain(e => (e as MyEvent)!.Value == 22);
    }

    [RunnableInDebugOnly]
    public async Task AppendsAndReplaysEventsFromLatestSnapshot()
    {
        var aggregateRoot = new OtherEventSourceAggregateRoot(AggregateId);
        aggregateRoot.ChangeValue(0);
        aggregateRoot.ChangeValue(11);
        aggregateRoot.ChangeValue(22);

        await AppendEvents<OtherEventSourceAggregateRoot>(aggregateRoot);

        var firstSnapshot = aggregateRoot.CreateSnapshot();
        await SaveSnapshot<OtherEventSourceAggregateRoot>(firstSnapshot);

        aggregateRoot.ChangeValue(33);
        aggregateRoot.ChangeValue(44);

        await AppendEvents<OtherEventSourceAggregateRoot>(aggregateRoot);

        var secondSnapshot = aggregateRoot.CreateSnapshot();
        await SaveSnapshot<OtherEventSourceAggregateRoot>(secondSnapshot);

        aggregateRoot.ChangeValue(55);
        aggregateRoot.ChangeValue(66);
        aggregateRoot.ChangeValue(77);

        await AppendEvents<OtherEventSourceAggregateRoot>(aggregateRoot);

        using var eventStream = await CreateEventStream<OtherEventSourceAggregateRoot>();
        var hasSnapshot = await eventStream.HasSnapshot(default);
        var snapshotFromEventStream = await eventStream.GetLatestSnapshot(default);

        hasSnapshot.Should().BeTrue();
        snapshotFromEventStream.Should().BeEquivalentTo(secondSnapshot);

        var asyncEventsFromStream = await eventStream.ReplayFromSnapshot(snapshotFromEventStream, default);
        var eventHistorySinceLatestSnapshot = await asyncEventsFromStream.ToListAsync();
        eventHistorySinceLatestSnapshot.Should().HaveCount(3);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as OtherEvent)!.Value == 55);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as OtherEvent)!.Value == 66);
        eventHistorySinceLatestSnapshot.Should().Contain(e => (e as OtherEvent)!.Value == 77);
    }

    [RunnableInDebugOnly]
    public async Task ReplaysAllEventsInOrderOfOccurrence()
    {
        var myEventSourcedAggregateRoot = new MyEventSourcedAggregateRoot("my-1");
        var otherEventSourcedAggregateRoot = new OtherEventSourceAggregateRoot("other-1");

        myEventSourcedAggregateRoot.ChangeValue(11);
        await Task.Delay(1);
        myEventSourcedAggregateRoot.ChangeValue(22);
        await Task.Delay(1);
        otherEventSourcedAggregateRoot.ChangeValue(111);
        await Task.Delay(1);
        myEventSourcedAggregateRoot.ChangeValue(33);
        await Task.Delay(1);
        otherEventSourcedAggregateRoot.ChangeValue(222);
        await Task.Delay(1);
        otherEventSourcedAggregateRoot.ChangeValue(333);

        await AppendEvents<MyEventSourcedAggregateRoot>(myEventSourcedAggregateRoot);
        await AppendEvents<OtherEventSourceAggregateRoot>(otherEventSourcedAggregateRoot);

        var eventPublisher = A.Fake<Func<object, CancellationToken, Task>>();
        var eventStore = new EventStoreStore(new EventStoreClientFactory(ConnectionString), TypeNameStrategy.Loose);
        await eventStore.ReplayAll(eventPublisher, default);

        A.CallTo(() => eventPublisher.Invoke(A<object>._, A<CancellationToken>._)).MustHaveHappened(6, Times.OrMore);

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

    private static async Task DeleteEventStream(EventStoreClient client, string streamName)
    {
        var stream = client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);
        if (await stream.ReadState == ReadState.Ok)
            await client.DeleteAsync(streamName, StreamState.Any);
    }

    private static Task<IEventStream> CreateEventStream<TAggregateRoot>()
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        var eventStoreClientFactory = new EventStoreClientFactory(ConnectionString);
        var eventStore = new EventStoreStore(eventStoreClientFactory, TypeNameStrategy.Loose);
        return eventStore.OpenStream<TAggregateRoot>(AggregateId, default);
    }

    private static async Task AppendEvents<TAggregateRoot>(EventSourcedAggregateRoot aggregateRoot)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        using var eventStream = await CreateEventStream<TAggregateRoot>();
        await eventStream.Append(
            [.. aggregateRoot.UncommittedEvents.OfType<VersionableEvent>()],
            aggregateRoot.Version,
            new Dictionary<string, object>(),
            default
        );
        aggregateRoot.CommitEvents();
    }

    private static async Task SaveSnapshot<TAggregateRoot>(ISnapshot snapshot)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        using var eventStream = await CreateEventStream<TAggregateRoot>();
        await eventStream.SaveSnapshot(snapshot, default);
    }
}