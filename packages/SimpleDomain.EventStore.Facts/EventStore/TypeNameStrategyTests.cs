namespace SimpleDomain.EventStore;

using FluentAssertions;
using TestDoubles;
using Xunit;

public class TypeNameStrategyTests
{
    [Fact]
    public void ReturnsLooseTypeName()
    {
        var typeNameStrategy = TypeNameStrategy.Loose;
        var instance = new MyEventSourcedAggregateRoot("1234");
        var typeName = typeNameStrategy.GetTypeName(instance);
        typeName.Should().Be("SimpleDomain.TestDoubles.MyEventSourcedAggregateRoot, SimpleDomain.TestDoubles");
    }

    [Fact]
    public void ReturnsStrictNameTypeName()
    {
        var typeNameStrategy = TypeNameStrategy.Strict;
        var instance = new MyEventSourcedAggregateRoot("1234");
        var typeName = typeNameStrategy.GetTypeName(instance);

        typeName.Should()
            .Contain("SimpleDomain.TestDoubles.MyEventSourcedAggregateRoot,").And
            .Contain("SimpleDomain.TestDoubles,").And
            .MatchRegex(@"Version=[0-9]+\.[0-9]+\.[0-9]+").And
            .Contain("Culture=neutral,").And
            .Contain("PublicKeyToken=null");
    }
}