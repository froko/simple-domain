namespace SimpleDomain.EventStore;

using TestDoubles;

public class SnapshotDescriptorTests
{
    [Fact]
    public void CreatesInstanceWithFactoryMethod()
    {
        const string AggregateId = "Test-1234";

        var now = DateTimeOffset.UtcNow;
        var snapshot = new MySnapshot(42, 1, now);
        var testee = SnapshotDescriptor.From<MyDynamicEventSourcedAggregateRoot>(AggregateId, snapshot);

        testee.AggregateType.Should().Be(typeof(MyDynamicEventSourcedAggregateRoot).FullName);
        testee.AggregateId.Should().Be(AggregateId);
        testee.Version.Should().Be(1);
        testee.Timestamp.Should().Be(now);
        testee.Snapshot.Should().BeSameAs(snapshot);
    }
}