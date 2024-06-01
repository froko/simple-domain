namespace SimpleDomain.TestDoubles;

public record MySnapshot(int Value, int Version, DateTimeOffset Timestamp) : ISnapshot;