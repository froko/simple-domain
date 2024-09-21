namespace GiftcardSample;

using SimpleDomain;

public sealed class Giftcard : EventSourcedAggregateRoot
{
    private decimal balance;
    private bool isActive;
    private DateTime validUntil;

    public Giftcard()
    {
        this.RegisterTransition<GiftcardCreated>(this.Apply);
        this.RegisterTransition<GiftcardActivated>(this.Apply);
        this.RegisterTransition<GiftcardLoaded>(this.Apply);
        this.RegisterTransition<GiftcardRedeemed>(this.Apply);
    }

    private Giftcard(int cardNumber, DateTime validUntil, decimal initialBalance) : this()
        => this.ApplyEvent(new GiftcardCreated(cardNumber, initialBalance, validUntil));

    public static Giftcard Create(int cardNumber, DateTime validUntil, decimal initialBalance = 0)
    {
        Guard.Argument(() => cardNumber >= 0, "Negative card number");
        Guard.Argument(() => validUntil >= DateTime.Today, "Already expired");
        Guard.Argument(() => initialBalance >= 0, "Negative initial balance");

        return new Giftcard(cardNumber, validUntil, initialBalance);
    }

    public void Activate()
    {
        Guard.State(() => this.validUntil >= DateTime.Today, "Already expired");
        Guard.State(() => !this.isActive, "Already active");

        this.ApplyEvent(new GiftcardActivated(this.Id));
    }

    public void Load(decimal amount)
    {
        Guard.Argument(() => amount > 0, "Negative amount");
        Guard.State(() => this.isActive, "Not active");
        Guard.State(() => this.validUntil >= DateTime.Today, "Already expired");

        this.ApplyEvent(new GiftcardLoaded(this.Id, amount));
    }

    public void Redeem(decimal amount)
    {
        Guard.Argument(() => amount > 0, "Negative amount");
        Guard.State(() => this.isActive, "Not active");
        Guard.State(() => this.validUntil >= DateTime.Today, "Already expired");
        Guard.State(() => amount <= this.balance, "Insufficient balance");

        this.ApplyEvent(new GiftcardRedeemed(this.Id, amount));
    }

    private void Apply(GiftcardCreated @event)
    {
        this.Id = $"{@event.CardNumber}";
        this.balance = @event.Balance;
        this.validUntil = @event.ValidUntil;
        this.isActive = false;
    }

    private void Apply(GiftcardActivated @event) => this.isActive = true;

    private void Apply(GiftcardLoaded @event) => this.balance += @event.Amount;

    private void Apply(GiftcardRedeemed @event) => this.balance -= @event.Amount;
}