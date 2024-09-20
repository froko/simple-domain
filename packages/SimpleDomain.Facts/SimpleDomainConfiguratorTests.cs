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
            .WithSnapshotStrategyFor<OtherEventSourceAggregateRoot>(20);

        var myEventSourcedAggregateRoot = new MyEventSourcedAggregateRoot();
        this.VerifyDoesNotNeedSnapshot(myEventSourcedAggregateRoot, 49);
        this.VerifyNeedsSnapshot(myEventSourcedAggregateRoot, 50);
        this.VerifyNeedsSnapshot(myEventSourcedAggregateRoot, 100);

        var otherEventSourcedAggregateRoot = new OtherEventSourceAggregateRoot();
        this.VerifyDoesNotNeedSnapshot(otherEventSourcedAggregateRoot, 19);
        this.VerifyNeedsSnapshot(otherEventSourcedAggregateRoot, 20);
        this.VerifyNeedsSnapshot(otherEventSourcedAggregateRoot, 40);
        this.VerifyDoesNotNeedSnapshot(otherEventSourcedAggregateRoot, 50);
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