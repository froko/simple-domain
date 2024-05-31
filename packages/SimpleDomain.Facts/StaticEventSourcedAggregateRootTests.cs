namespace SimpleDomain;

using TestDoubles;

public class StaticEventSourcedAggregateRootTests
{
    private readonly MyStaticEventSourcedAggregateRoot testee = new();


    [Fact]
    public void InitialVersionEqualsMinusOne() => this.testee.Version.Should().Be(-1);

    [Fact]
    public void AppliesChange()
    {
        var @event = new ValueEvent(11);
        this.testee.ApplyEvent(@event);

        this.testee.Value.Should().Be(11);
    }

    [Fact]
    public void AppliedChangeIsAddedToUncommittedEvents()
    {
        var @event = new ValueEvent(11);
        this.testee.ApplyEvent(@event);

        this.testee.UncommittedEvents.OfType<VersionableEvent>().Should().Contain(e => e.InnerEvent == @event);
    }

    [Fact]
    public void AggregateVersionIsIncremented_WhenChangeIsApplied()
    {
        var firstEvent = new ValueEvent(11);
        var secondEvent = new ValueEvent(22);

        this.testee.ApplyEvent(firstEvent);
        this.testee.ApplyEvent(secondEvent);

        this.testee.Version.Should().Be(1);
    }

    [Fact]
    public void IncrementedAggregateVersionIsAppliedToEvent()
    {
        var firstEvent = new ValueEvent(11);
        var secondEvent = new ValueEvent(22);

        this.testee.ApplyEvent(firstEvent);
        this.testee.ApplyEvent(secondEvent);

        this.testee.UncommittedEvents.OfType<VersionableEvent>().First().Version.Should().Be(0);
        this.testee.UncommittedEvents.OfType<VersionableEvent>().Last().Version.Should().Be(1);
    }

    [Fact]
    public void CommitsUncommittedEvents()
    {
        var @event = new ValueEvent(11);
        this.testee.ApplyEvent(@event);

        this.testee.CommitEvents();

        this.testee.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void LoadsFromEventHistory()
    {
        var eventHistory = EventHistory.Create(new ValueEvent(11), new ValueEvent(22), new ValueEvent(33));
        this.testee.LoadFromEventHistory(eventHistory);

        this.testee.Value.Should().Be(33);
    }

    [Fact]
    public void LoadsFromSnapshot()
    {
        var eventHistory = EventHistory.Create(new ValueEvent(11), new ValueEvent(22), new ValueEvent(33));
        this.testee.LoadFromEventHistory(eventHistory);

        var snapshot = this.testee.CreateSnapshot();
        var newTestee = new MyDynamicEventSourcedAggregateRoot();
        newTestee.LoadFromSnapshot(snapshot);

        newTestee.Value.Should().Be(33);
    }
}
