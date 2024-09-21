namespace GiftcardSample;

using Microsoft.Extensions.DependencyInjection;
using SimpleDomain;
using SimpleDomain.EventStore;

public class SpecHarness
{
    private readonly Bus bus;
    private readonly IEventStore eventStore;

    public SpecHarness(IServiceProvider provider)
    {
        this.eventStore = provider.GetRequiredService<IEventStore>();
        this.bus = provider.GetRequiredService<Bus>();
        this.UseCases = provider.GetRequiredService<UseCases>();

        provider.GetRequiredService<EventHandlers>().Subscribe();
    }

    public UseCases UseCases { get; }

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
        await Task.WhenAll(events.Select(this.bus.Publish));
    }
}