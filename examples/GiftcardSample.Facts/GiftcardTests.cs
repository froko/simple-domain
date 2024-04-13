namespace GiftcardSample;

public class GiftcardTests : EventSourcedAggregateRootFixture<Giftcard>
{
    private const int CardNumber = 12345;
    private static readonly string CardId = $"{CardNumber}";
    private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

    [Fact]
    public void HasParameterlessConstructor()
    {
        var giftcard = new Giftcard();
        giftcard.Should().NotBeNull();
    }

    [Fact]
    public void CreatesGiftcard()
    {
        this.Create(() => Giftcard.Create(CardNumber, ValidUntil));
        this.ShouldEmitEventLike<GiftcardCreated>(e =>
            e is { CardNumber: CardNumber, Balance: 0m } && e.ValidUntil == ValidUntil && e.CardId == CardId);
    }

    [Fact]
    public void CreatesGiftcardWithInitialBalance()
    {
        this.Create(() => Giftcard.Create(CardNumber, ValidUntil, 100m));
        this.ShouldEmitEventLike(new GiftcardCreated(CardNumber, 100m, ValidUntil));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToCreateGiftcardWithNegativeNumber()
    {
        this.Create(() => Giftcard.Create(-1, ValidUntil));
        this.ShouldFailWith<ArgumentException>("Negative card number");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToCreateAnExpiredGiftcard()
    {
        this.Create(() => Giftcard.Create(CardNumber, DateTime.Today.AddDays(-1)));
        this.ShouldFailWith<ArgumentException>("Already expired");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToCreateGiftcardWithNegativeBalance()
    {
        this.Create(() => Giftcard.Create(CardNumber, ValidUntil, -13m));
        this.ShouldFailWith<ArgumentException>("Negative initial balance");
    }

    [Fact]
    public void ActivatesGiftcard()
    {
        this.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil));
        this.Execute(g => g.Activate());
        this.ShouldEmitEventLike(new GiftcardActivated(CardId));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToActivateAnAlreadyActivatedGiftcard()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, ValidUntil),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Activate());
        this.ShouldFailWith<InvalidOperationException>("Already active");
    }

    [Fact]
    public void LoadsGiftcard()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, ValidUntil),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Load(10m));
        this.ShouldEmitEventLike(new GiftcardLoaded(CardId, 10m));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToLoadGiftcardWithNegativeAmount()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, DateTime.Today.AddDays(-1)),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Load(-10m));
        this.ShouldFailWith<ArgumentException>("Negative amount");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToLoadANonActivatedGiftcard()
    {
        this.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil));
        this.Execute(g => g.Load(10m));
        this.ShouldFailWith<InvalidOperationException>("Not active");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToLoadAnExpiredGiftcard()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, DateTime.Today.AddDays(-1)),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Load(10m));
        this.ShouldFailWith<InvalidOperationException>("Already expired");
    }


    [Fact]
    public void RedeemsGiftcard()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, ValidUntil),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Redeem(10m));
        this.ShouldEmitEventLike(new GiftcardRedeemed(CardId, 10m));
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemGiftcardWithNegativeAmount()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, ValidUntil),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Redeem(-10m));
        this.ShouldFailWith<ArgumentException>("Negative amount");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemANonActivatedGiftcard()
    {
        this.LoadFromHistory(new GiftcardCreated(CardNumber, 100m, ValidUntil));
        this.Execute(g => g.Redeem(10m));
        this.ShouldFailWith<InvalidOperationException>("Not active");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemAnExpiredGiftcard()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, DateTime.Today.AddDays(-1)),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Redeem(10m));
        this.ShouldFailWith<InvalidOperationException>("Already expired");
    }

    [Fact]
    public void ThrowsExceptionWhenTryingToRedeemMoreThanTheBalance()
    {
        this.LoadFromHistory(
            new GiftcardCreated(CardNumber, 100m, ValidUntil),
            new GiftcardActivated(CardId));
        this.Execute(g => g.Redeem(110m));
        this.ShouldFailWith<InvalidOperationException>("Insufficient balance");
    }
}
