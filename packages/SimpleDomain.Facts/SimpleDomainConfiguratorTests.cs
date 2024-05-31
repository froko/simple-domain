namespace SimpleDomain;

using EventStore;
using EventStore.Persistence;
using Microsoft.Extensions.DependencyInjection;
using TestDoubles;

public class SimpleDomainConfiguratorTests
{
    private readonly IServiceCollection serviceCollection = new ServiceCollection();
    private readonly SimpleDomainConfigurator testee;

    public SimpleDomainConfiguratorTests() => this.testee = new SimpleDomainConfigurator(this.serviceCollection);

    [Fact]
    public void ConfiguresInMemoryEventStore()
    {
        this.testee.UseInMemoryEventStore();

        this.serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IEventStore));
        this.serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IEventSourcedRepository));
        this.testee.Get<List<EventDescriptor>>(InMemoryEventStore.EventDescriptors).Should().NotBeNull();
        this.testee.Get<List<SnapshotDescriptor>>(InMemoryEventStore.SnapshotDescriptors).Should().NotBeNull();
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
