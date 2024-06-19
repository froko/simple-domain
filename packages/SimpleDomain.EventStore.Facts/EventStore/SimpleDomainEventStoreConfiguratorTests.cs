namespace SimpleDomain.EventStore;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TestDoubles;
using Xunit;

public class SimpleDomainEventStoreConfiguratorTests
{
    private const string ConnectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

    private readonly IServiceCollection serviceCollection = new ServiceCollection();
    private readonly SimpleDomainEventStoreConfigurator testee;

    public SimpleDomainEventStoreConfiguratorTests() =>
        this.testee = new SimpleDomainEventStoreConfigurator(this.serviceCollection);

    [Fact]
    public void ConfiguresEventStore()
    {
        this.testee.UseEventStore(ConnectionString);

        this.serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IEventStore));
        this.serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IEventSourcedRepository));
        this.serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(TypeNameStrategy));
    }

    [Fact]
    public void ConfiguresSnapshotStrategies()
    {
        this.testee
            .WithGlobalSnapshotStrategy(50)
            .WithSnapshotStrategyFor<MyDynamicEventSourcedAggregateRoot>(20);

        var dynamicEventSourceAggregateRoot = new MyDynamicEventSourcedAggregateRoot();
        this.VerifyDoesNotNeedSnapshot(dynamicEventSourceAggregateRoot, 19);
        this.VerifyNeedsSnapshot(dynamicEventSourceAggregateRoot, 20);
        this.VerifyNeedsSnapshot(dynamicEventSourceAggregateRoot, 40);
        this.VerifyDoesNotNeedSnapshot(dynamicEventSourceAggregateRoot, 50);

        var staticEventSourcedAggregateRoot = new MyStaticEventSourcedAggregateRoot();
        this.VerifyDoesNotNeedSnapshot(staticEventSourcedAggregateRoot, 49);
        this.VerifyNeedsSnapshot(staticEventSourcedAggregateRoot, 50);
        this.VerifyNeedsSnapshot(staticEventSourcedAggregateRoot, 100);
    }

    [Fact]
    public void AddsTheConfigurationAsSingletonOnComplete()
    {
        this.serviceCollection.Should().NotContain(x => x.ServiceType == typeof(IHaveEventStoreConfiguration));

        this.testee.Complete();

        this.serviceCollection.Should().ContainSingle(x =>
            x.ServiceType == typeof(IHaveEventStoreConfiguration) && x.Lifetime == ServiceLifetime.Singleton);
    }

    private void VerifyNeedsSnapshot<TAggregateRoot>(TAggregateRoot aggregateRoot, int version)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.testee
            .GetSnapshotStrategy<TAggregateRoot>()
            .NeedsSnapshot(aggregateRoot.WithVersion(version))
            .Should().BeTrue();

    private void VerifyDoesNotNeedSnapshot<TAggregateRoot>(TAggregateRoot aggregateRoot, int version)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.testee
            .GetSnapshotStrategy<TAggregateRoot>()
            .NeedsSnapshot(aggregateRoot.WithVersion(version))
            .Should().BeFalse();

}