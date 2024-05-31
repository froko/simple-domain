namespace SimpleDomain.EventStore;

using TestDoubles;

public class EventHistoryTests
{
    [Fact]
    public void CreatesAnEmptyEventHistoryByConstructor()
    {
        var testee = new EventHistory(Enumerable.Empty<IEvent>());

        testee.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void CanCreateAnEmptyEventHistoryByFactoryMethod()
    {
        var testee = EventHistory.Create();

        testee.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void ReturnsProvidedEvents_WhenCreatedByConstructor()
    {
        var events = new[] { new ValueEvent(11), new ValueEvent(22) };
        var testee = new EventHistory(events);

        testee.Should().HaveCount(2);
        testee.First().As<ValueEvent>().Value.Should().Be(11);
        testee.Last().As<ValueEvent>().Value.Should().Be(22);
    }

    [Fact]
    public void ReturnsProvidedEvents_WhenCreatedByFactoryMethod()
    {
        var testee = EventHistory.Create(new ValueEvent(11), new ValueEvent(22));

        testee.Should().HaveCount(2);
        testee.First().As<ValueEvent>().Value.Should().Be(11);
        testee.Last().As<ValueEvent>().Value.Should().Be(22);
    }
}
