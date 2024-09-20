namespace SimpleDomain.EventStore;

using TestDoubles;

public class SnapshotStrategyTests
{
    [Fact]
    public void CreatesUntypedInstance()
    {
        var testee = new SnapshotStrategy(10);
        testee.Should().NotBeNull();
    }

    [Fact]
    public void CreatesTypedInstance()
    {
        var testee = new SnapshotStrategy(10, typeof(MyEventSourcedAggregateRoot));
        testee.Should().NotBeNull();
    }

    [Fact]
    public void VerifiesApplicabilityOfTypedSnapshotStrategyToGivenAggregateRoot()
    {
        var testee = new SnapshotStrategy(10, typeof(MyEventSourcedAggregateRoot));
        testee.AppliesToThisAggregateRoot<MyEventSourcedAggregateRoot>().Should().BeTrue();
        testee.AppliesToThisAggregateRoot<OtherEventSourceAggregateRoot>().Should().BeFalse();
        testee.AppliesToThisAggregateRoot<EventSourcedAggregateRoot>().Should().BeFalse();
    }

    [Fact]
    public void VerifiesIfAggregateRootNeedsSnapshot()
    {
        var testee = new SnapshotStrategy(10, typeof(MyEventSourcedAggregateRoot));

        var aggregateRoot = new MyEventSourcedAggregateRoot();
        testee.NeedsSnapshot(aggregateRoot.WithVersion(9)).Should().BeFalse();
        testee.NeedsSnapshot(aggregateRoot.WithVersion(10)).Should().BeTrue();
        testee.NeedsSnapshot(aggregateRoot.WithVersion(11)).Should().BeFalse();
        testee.NeedsSnapshot(aggregateRoot.WithVersion(20)).Should().BeTrue();
    }
}