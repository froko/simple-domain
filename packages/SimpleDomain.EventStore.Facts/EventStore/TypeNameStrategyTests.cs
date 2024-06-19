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
        var instance = new MyStaticEventSourcedAggregateRoot("1234");
        var typeName = typeNameStrategy.GetTypeName(instance);
        typeName.Should().Be("SimpleDomain.TestDoubles.MyStaticEventSourcedAggregateRoot, SimpleDomain.TestDoubles");
    }

    [Fact]
    public void ReturnsStrictNameTypeName()
    {
        var typeNameStrategy = TypeNameStrategy.Strict;
        var instance = new MyStaticEventSourcedAggregateRoot("1234");
        var typeName = typeNameStrategy.GetTypeName(instance);

        typeName.Should()
            .Contain("SimpleDomain.TestDoubles.MyStaticEventSourcedAggregateRoot,").And
            .Contain("SimpleDomain.TestDoubles,").And
            .MatchRegex(@"Version=[0-9]+\.[0-9]+\.[0-9]+").And
            .Contain("Culture=neutral,").And
            .Contain("PublicKeyToken=null");
    }
}