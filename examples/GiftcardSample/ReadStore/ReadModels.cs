namespace GiftcardSample.ReadStore;

public enum GiftcardStatus
{
    NotActive,
    Active
}

public record GiftcardSummary(int Count, decimal TotalAmount, decimal SumOfExecutedPayments);

public record GiftcardOverview(
    string CardId,
    int CardNumber,
    decimal Balance,
    DateTime ValidUntil,
    GiftcardStatus Status);

public record GiftcardTransaction(
    string CardId,
    int CardNumber,
    DateTime ValutaDate,
    string Event,
    decimal Balance,
    decimal Amount,
    int Revision);