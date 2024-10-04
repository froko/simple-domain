namespace GiftcardSample;

using FluentAssertions;
using LambdaTale;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ReadStore;
using SimpleDomain;

public class MassTransitGiftcardFeatures
{
    private const int CardNumber = 12345;
    private const decimal Amount = 100m;

    private static readonly string CardId = $"{CardNumber}";
    private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

    private readonly SpecHarness harness;
    private readonly IReadStore readStore;

    public MassTransitGiftcardFeatures()
    {
        var provider = new ServiceCollection()
            .AddSimpleDomain(c => c.UseInMemoryEventStore())
            .AddReadStore()
            .AddMassTransitTestHarness(c =>
            {
                c.AddCommandHandlers();
                c.AddEventHandlers();
                c.UsingInMemory((context, configuration) => { configuration.ConfigureEndpoints(context); });
            })
            .BuildServiceProvider(true);

        this.harness = new SpecHarness(provider);
        this.readStore = provider.GetRequiredService<IReadStore>();
    }

    [Background]
    public Task StartUp() => this.harness.StartUp();

    [Scenario]
    public void CreateGiftcard()
    {
        "When a giftcard is created"
            .x(async () => await this.harness.Send(new CreateGiftcard(CardNumber, Amount, ValidUntil)));

        "And message processing is completed".x(this.harness.WaitForMessageProcessing);

        "Then the new giftcard is persisted in the overview read store"
            .x(async () => (await this.readStore.GetOverview(CardId)).Should()
                .BeEquivalentTo(
                    new GiftcardOverview(CardId, CardNumber, Amount, ValidUntil, GiftcardStatus.NotActive)));

        "And a new giftcard transaction is being written"
            .x(async () => (await this.readStore.GetTransactions(CardId)).Should()
                .ContainEquivalentOf(
                    new GiftcardTransaction(CardId, CardNumber, DateTime.Today, "Created", Amount, 0, 0)));

        "And the giftcard summary is updated"
            .x(async () => (await this.readStore.GetSummary()).Should()
                .BeEquivalentTo(new GiftcardSummary(1, Amount, 0)));
    }

    [Scenario]
    public void ActivateGiftcard()
    {
        "Given a giftcard is created"
            .x(async () => await this.harness.Replay(CardId, new GiftcardCreated(CardNumber, Amount, ValidUntil)));

        "When the giftcard is activated"
            .x(async () => await this.harness.Send(new ActivateGiftcard(CardId)));

        "And message processing is completed".x(this.harness.WaitForMessageProcessing);

        "Then the giftcard status is updated in the overview read store"
            .x(async () =>
                (await this.readStore.GetOverview(CardId)).Should()
                .BeEquivalentTo(new { Status = GiftcardStatus.Active }));

        "And a new giftcard transaction is being written"
            .x(async () => (await this.readStore.GetTransactions(CardId)).Should()
                .ContainEquivalentOf(new { Event = "Activated" }));
    }

    [Scenario]
    public void RedeemGiftcard()
    {
        "Given an activated giftcard"
            .x(async () => await this.harness.Replay(
                CardId,
                new GiftcardCreated(CardNumber, Amount, ValidUntil),
                new GiftcardActivated(CardId)));

        "When the giftcard is redeemed"
            .x(async () => await this.harness.Send(new RedeemGiftcard(CardId, 25)));

        "And message processing is completed".x(this.harness.WaitForMessageProcessing);

        "Then the giftcard balance is updated in the overview read store"
            .x(async () =>
                (await this.readStore.GetOverview(CardId)).Should()
                .BeEquivalentTo(new { Balance = 75 }));

        "And a giftcard transaction is being written"
            .x(async () => (await this.readStore.GetTransactions(CardId)).Should()
                .ContainEquivalentOf(new { Event = "Redeemed", Amount = 25 }));

        "And the giftcard summary is updated"
            .x(async () => (await this.readStore.GetSummary()).Should()
                .BeEquivalentTo(new GiftcardSummary(1, 75, 25)));
    }

    [Scenario]
    public void LoadGiftcard()
    {
        "Given a used giftcard"
            .x(async () => await this.harness.Replay(
                CardId,
                new GiftcardCreated(CardNumber, Amount, ValidUntil),
                new GiftcardActivated(CardId),
                new GiftcardRedeemed(CardId, 25)));

        "When the giftcard is loaded"
            .x(async () => await this.harness.Send(new LoadGiftcard(CardId, 50)));

        "And message processing is completed".x(this.harness.WaitForMessageProcessing);

        "Then the giftcard balance is updated in the overview read store"
            .x(async () =>
                (await this.readStore.GetOverview(CardId)).Should()
                .BeEquivalentTo(new { Balance = 125 }));

        "And a giftcard transaction is being written"
            .x(async () => (await this.readStore.GetTransactions(CardId)).Should()
                .ContainEquivalentOf(new { Event = "Loaded", Amount = 50 }));

        "And the giftcard summary is updated"
            .x(async () => (await this.readStore.GetSummary()).Should()
                .BeEquivalentTo(new GiftcardSummary(1, 125, 25)));
    }
}