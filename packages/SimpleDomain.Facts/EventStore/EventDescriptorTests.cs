namespace SimpleDomain.EventStore;

using TestDoubles;

public class EventDescriptorTests
{
    [Fact]
    public void CreatesInstanceWithFactoryMethod()
    {
        const string AggregateId = "Test-1234";

        var now = DateTimeOffset.UtcNow;
        var versionableEvent = new VersionableEvent(new MyEvent(42), 1, now);
        var headers = new Dictionary<string, object> { { "UserName", "Patrick" }, { "MagicNumber", 42 } };
        var testee = EventDescriptor.From<MyEventSourcedAggregateRoot>(AggregateId, versionableEvent, headers);

        testee.AggregateType.Should().Be(typeof(MyEventSourcedAggregateRoot).FullName);
        testee.AggregateId.Should().Be(AggregateId);
        testee.Version.Should().Be(1);
        testee.Timestamp.Should().Be(now);
        testee.EventType.Should().Be("SimpleDomain.TestDoubles.MyEvent");
        testee.Event.Should().BeSameAs(versionableEvent.InnerEvent);
        testee.Headers.Should().Contain("UserName", "Patrick").And.Contain("MagicNumber", 42);
    }
}