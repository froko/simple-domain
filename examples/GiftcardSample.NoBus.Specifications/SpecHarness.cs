namespace GiftcardSample;

using Microsoft.Extensions.DependencyInjection;
using ReadStore;
using SimpleDomain;
using SimpleDomain.EventStore;

public class SpecHarness(IServiceProvider provider)
{
    private readonly IEventStore eventStore = provider.GetRequiredService<IEventStore>();
    private readonly IWriteStore writeStore = provider.GetRequiredService<IWriteStore>();

    public UseCases UseCases { get; } = provider.GetRequiredService<UseCases>();

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
        await Task.WhenAll(events.Select(this.Publish));
    }

    private Task Publish(object @event) =>
        @event switch
        {
            GiftcardCreated giftcardCreated => this.writeStore.Add(
                giftcardCreated.CardId,
                giftcardCreated.CardNumber,
                giftcardCreated.Balance,
                giftcardCreated.ValidUntil),
            GiftcardActivated giftcardActivated => this.writeStore.Activate(giftcardActivated.CardId),
            GiftcardRedeemed giftcardRedeemed => this.writeStore.Redeem(giftcardRedeemed.CardId,
                giftcardRedeemed.Amount),
            GiftcardLoaded giftcardLoaded => this.writeStore.Load(giftcardLoaded.CardId, giftcardLoaded.Amount),
            _ => Task.CompletedTask
        };
}