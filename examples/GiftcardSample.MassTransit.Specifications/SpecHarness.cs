namespace GiftcardSample;

using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimpleDomain;
using SimpleDomain.EventStore;

public class SpecHarness(IServiceProvider provider)
{
    private readonly IEventStore eventStore = provider.GetRequiredService<IEventStore>();
    private readonly ITestHarness harness = provider.GetRequiredService<ITestHarness>();

    public Task StartUp() => this.harness.Start();

    public Task WaitForMessageProcessing() => this.harness.InactivityTask;

    public Task Send<T>(T message) where T : class => this.harness.Bus.Publish(message);

    public async Task Replay(string aggregateId, params object[] events)
    {
        var expectedVersion = events.Length - 1;
        var version = 0;

        var headers = new Dictionary<string, object>();
        var versionableEvents = events
            .Select(@event => new VersionableEvent(@event, version++, DateTimeOffset.Now))
            .ToList();

        using var eventStream = await this.eventStore.OpenStream<Giftcard>(aggregateId, CancellationToken.None);
        await eventStream.Append(versionableEvents, expectedVersion, headers, CancellationToken.None);
        await this.harness.Bus.PublishBatch(events);
    }
}