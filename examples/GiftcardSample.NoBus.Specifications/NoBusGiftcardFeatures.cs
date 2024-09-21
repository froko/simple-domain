﻿namespace GiftcardSample;

using FluentAssertions;
using LambdaTale;
using Microsoft.Extensions.DependencyInjection;
using ReadStore;
using SimpleDomain;

public class NoBusGiftcardFeatures
{
    private const int CardNumber = 12345;
    private const decimal Amount = 100m;

    private static readonly string CardId = $"{CardNumber}";
    private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

    private readonly SpecHarness harness;
    private readonly IReadStore readStore;

    public NoBusGiftcardFeatures()
    {
        var provider = new ServiceCollection()
            .AddSimpleDomain(c => c.UseInMemoryEventStore())
            .AddGiftcardUseCases()
            .AddReadStore()
            .BuildServiceProvider(true);

        this.harness = new SpecHarness(provider);
        this.readStore = provider.GetRequiredService<IReadStore>();
    }

    [Scenario]
    public void CreateGiftcard()
    {
        "When a giftcard is created"
            .x(async () => await this.harness.UseCases.Create(CardNumber, ValidUntil, Amount));

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
            .x(async () => await this.harness.UseCases.Activate(CardId));

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
            .x(async () => await this.harness.UseCases.Redeem(CardId, 25));

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
            .x(async () => await this.harness.UseCases.Load(CardId, 50));

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