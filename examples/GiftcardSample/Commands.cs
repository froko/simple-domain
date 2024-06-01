namespace GiftcardSample;

public record CreateGiftcard(int CardNumber, decimal Balance, DateTime ValidUntil);

public record ActivateGiftcard(string CardId);

public record RedeemGiftcard(string CardId, decimal Amount);

public record LoadGiftcard(string CardId, decimal Amount);