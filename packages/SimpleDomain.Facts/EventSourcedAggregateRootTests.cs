namespace SimpleDomain;

using TestDoubles;

public class EventSourcedAggregateRootTests
{
    private readonly MyEventSourcedAggregateRoot testee = new();


    [Fact]
    public void InitialVersionEqualsMinusOne() => this.testee.Version.Should().Be(-1);

    [Fact]
    public void AppliesChange()
    {
        var @event = new MyEvent(11);
        this.testee.ApplyEvent(@event);

        this.testee.Value.Should().Be(11);
    }

    [Fact]
    public void AppliedChangeIsAddedToUncommittedEvents()
    {
        var @event = new MyEvent(11);
        this.testee.ApplyEvent(@event);

        this.testee.UncommittedEvents.OfType<VersionableEvent>().Should()
            .Contain(e => ReferenceEquals(e.InnerEvent, @event));
    }

    [Fact]
    public void AggregateVersionIsIncrementedWhenChangeIsApplied()
    {
        var firstEvent = new MyEvent(11);
        var secondEvent = new MyEvent(22);

        this.testee.ApplyEvent(firstEvent);
        this.testee.ApplyEvent(secondEvent);

        this.testee.Version.Should().Be(1);
    }

    [Fact]
    public void IncrementedAggregateVersionIsAppliedToEvent()
    {
        var firstEvent = new MyEvent(11);
        var secondEvent = new MyEvent(22);

        this.testee.ApplyEvent(firstEvent);
        this.testee.ApplyEvent(secondEvent);

        this.testee.UncommittedEvents.OfType<VersionableEvent>().First().Version.Should().Be(0);
        this.testee.UncommittedEvents.OfType<VersionableEvent>().Last().Version.Should().Be(1);
    }

    [Fact]
    public void CommitsUncommittedEvents()
    {
        var @event = new MyEvent(11);
        this.testee.ApplyEvent(@event);

        this.testee.CommitEvents();

        this.testee.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadsFromEventHistory()
    {
        var events = new[] { new MyEvent(11), new MyEvent(22), new MyEvent(33) }.ToAsyncEnumerable();
        await this.testee.LoadFromEventHistory(events);

        this.testee.Value.Should().Be(33);
    }

    [Fact]
    public async Task LoadsFromSnapshot()
    {
        var events = new[] { new MyEvent(11), new MyEvent(22), new MyEvent(33) }.ToAsyncEnumerable();
        await this.testee.LoadFromEventHistory(events);

        var snapshot = this.testee.CreateSnapshot();
        var newTestee = new MyEventSourcedAggregateRoot();
        newTestee.LoadFromSnapshot(snapshot);

        newTestee.Value.Should().Be(33);
    }
}