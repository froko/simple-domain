namespace GiftcardSample;

using SimpleDomain;

public record GiftcardCreated(int CardNumber, decimal Balance, DateTime ValidUntil) : IEvent
{
    public string CardId => $"{this.CardNumber}";
}

public record GiftcardActivated(string CardId) : IEvent;

public record GiftcardRedeemed(string CardId, decimal Amount) : IEvent;

public record GiftcardLoaded(string CardId, decimal Amount) : IEvent;
