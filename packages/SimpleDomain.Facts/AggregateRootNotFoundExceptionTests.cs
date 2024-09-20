namespace SimpleDomain;

using TestDoubles;

public class AggregateRootNotFoundExceptionTests
{
    [Fact]
    public void CreatesInstance()
    {
        var aggregateKey = "12345";
        var testee = new AggregateRootNotFoundException(typeof(MyEventSourcedAggregateRoot), aggregateKey);

        testee.Message.Should()
            .Be(
                "An aggregate of type SimpleDomain.TestDoubles.MyEventSourcedAggregateRoot with key 12345 could not be found.");
    }
}