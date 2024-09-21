namespace GiftcardSample;

public record GiftcardCreated(int CardNumber, decimal Balance, DateTime ValidUntil)
{
    public string CardId => $"{this.CardNumber}";
}

public record GiftcardActivated(string CardId);

public record GiftcardRedeemed(string CardId, decimal Amount);

public record GiftcardLoaded(string CardId, decimal Amount);