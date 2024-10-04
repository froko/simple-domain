namespace SimpleDomain.EventStore.Persistence;

using global::EventStore.Client;

internal class SnapshotDataBuilder
{
    private readonly ISnapshot snapshot;
    private readonly Uuid snapshotId;
    private readonly string snapshotName;

    private SnapshotDataBuilder(ISnapshot snapshot, TypeNameStrategy typeNameStrategy)
    {
        this.snapshot = snapshot;
        this.snapshotId = Uuid.NewUuid();
        this.snapshotName = typeNameStrategy.GetTypeName(snapshot);
    }

    public static SnapshotDataBuilder Initialize(ISnapshot snapshot, TypeNameStrategy typeNameStrategy) =>
        new(snapshot, typeNameStrategy);

    public EventData Build() => new(this.snapshotId, this.snapshotName, this.snapshot.AsByteArray());
}