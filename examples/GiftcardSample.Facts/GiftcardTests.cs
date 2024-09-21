namespace GiftcardSample;

using FluentAssertions;
using SimpleDomain;
using Xunit;

public class GiftcardTests
{
    private const int CardNumber = 12345;
    private static readonly string CardId = $"{CardNumber}";
    private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

    private readonly EventSourcedAggregateRootHarness<Giftcard> harness = new();

    [Fact]
    public void HasParameterlessConstructor()
    {
        var giftcard = new Giftcard();
        giftcard.Should().NotBeNull();
    }

    [Fact]
    public void CreatesGiftcard()
    {
        this.harness.Create(() => Giftcard.Create(CardNumber, ValidUntil));
        this.harness.ShouldEmitEventLike<GiftcardCreated>(e =>
            e is { CardNumber: CardNumber, Balance: 0m } && e.ValidUntil == ValidUntil
        );
    }

    [Fact]
    public void CreatesGiftcardWithInitialBalance()
    {
        this.harness.Create(() => Giftcard.Create(CardNumber, ValidUntil, 100m));
        this.harness.ShouldEmitEventLike(new GiftcardCreated(CardNumber, 100m, ValidUntil));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToCreateGiftcardWithNegativeNumber()
    {
        this.harness.Create(() => Giftcard.Create(-1, ValidUntil));
        this.harness.ShouldFailWith<ArgumentException>("Negative card number");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToCreateAnExpiredGiftcard()
    {
        this.harness.Create(() => Giftcard.Create(CardNumber, DateTime.Today.AddDays(-1)));
        this.harness.ShouldFailWith<ArgumentException>("Already expired");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToCreateGiftcardWithNegativeBalance()
    {
        this.harness.Create(() => Giftcard.Create(CardNumber, ValidUntil, -13m));
        this.harness.ShouldFailWith<ArgumentException>("Negative initial balance");
    }

    [Fact]
    public void ActivatesGiftcard()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil));
        this.harness.Execute(g => g.Activate());
        this.harness.ShouldEmitEventLike(new GiftcardActivated(CardId));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToActivateAnAlreadyActivatedGiftcard()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil), new GiftcardActivated(CardId));
        this.harness.Execute(g => g.Activate());
        this.harness.ShouldFailWith<InvalidOperationException>("Already active");
    }

    [Fact]
    public void LoadsGiftcard()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil), new GiftcardActivated(CardId));
        this.harness.Execute(g => g.Load(10m));
        this.harness.ShouldEmitEventLike(new GiftcardLoaded(CardId, 10m));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToLoadGiftcardWithNegativeAmount()
    {
        this.harness.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, DateTime.Today.AddDays(-1)),
            new GiftcardActivated(CardId)
        );
        this.harness.Execute(g => g.Load(-10m));
        this.harness.ShouldFailWith<ArgumentException>("Negative amount");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToLoadANonActivatedGiftcard()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil));
        this.harness.Execute(g => g.Load(10m));
        this.harness.ShouldFailWith<InvalidOperationException>("Not active");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToLoadAnExpiredGiftcard()
    {
        this.harness.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, DateTime.Today.AddDays(-1)),
            new GiftcardActivated(CardId)
        );
        this.harness.Execute(g => g.Load(10m));
        this.harness.ShouldFailWith<InvalidOperationException>("Already expired");
    }

    [Fact]
    public void RedeemsGiftcard()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil), new GiftcardActivated(CardId));
        this.harness.Execute(g => g.Redeem(10m));
        this.harness.ShouldEmitEventLike(new GiftcardRedeemed(CardId, 10m));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemGiftcardWithNegativeAmount()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil), new GiftcardActivated(CardId));
        this.harness.Execute(g => g.Redeem(-10m));
        this.harness.ShouldFailWith<ArgumentException>("Negative amount");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemANonActivatedGiftcard()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil));
        this.harness.Execute(g => g.Redeem(10m));
        this.harness.ShouldFailWith<InvalidOperationException>("Not active");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemAnExpiredGiftcard()
    {
        this.harness.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, DateTime.Today.AddDays(-1)),
            new GiftcardActivated(CardId)
        );
        this.harness.Execute(g => g.Redeem(10m));
        this.harness.ShouldFailWith<InvalidOperationException>("Already expired");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemMoreThanTheBalance()
    {
        this.harness.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil), new GiftcardActivated(CardId));
        this.harness.Execute(g => g.Redeem(110m));
        this.harness.ShouldFailWith<InvalidOperationException>("Insufficient balance");
    }
}