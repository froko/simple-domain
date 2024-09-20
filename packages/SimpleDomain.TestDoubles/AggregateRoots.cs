namespace SimpleDomain.TestDoubles;

public class MyEventSourcedAggregateRoot : EventSourcedAggregateRoot
{
    public MyEventSourcedAggregateRoot() => this.RegisterTransition<MyEvent>(this.Apply);

    public MyEventSourcedAggregateRoot(string aggregateId) : this() => this.Id = aggregateId;

    public int Value { get; private set; }

    public override ISnapshot CreateSnapshot() => new MySnapshot(this.Value, this.Version, DateTimeOffset.UtcNow);

    public override void LoadFromSnapshot(ISnapshot snapshot)
    {
        if (snapshot is not MySnapshot mySnapshot) return;

        this.Version = mySnapshot.Version;
        this.Value = mySnapshot.Value;
    }

    public void ChangeValue(int value) => this.ApplyEvent(new MyEvent(value));

    public new void ApplyEvent(object @event) => base.ApplyEvent(@event);

    private void Apply(MyEvent @event) => this.Value = @event.Value;
}

public class OtherEventSourceAggregateRoot : EventSourcedAggregateRoot
{
    public OtherEventSourceAggregateRoot() => this.RegisterTransition<OtherEvent>(this.Apply);

    public OtherEventSourceAggregateRoot(string aggregateId) : this() => this.Id = aggregateId;

    public int Value { get; private set; }

    public override ISnapshot CreateSnapshot() => new OtherSnapshot(this.Value, this.Version, DateTimeOffset.UtcNow);

    public override void LoadFromSnapshot(ISnapshot snapshot)
    {
        if (snapshot is not OtherSnapshot otherSnapshot) return;

        this.Version = otherSnapshot.Version;
        this.Value = otherSnapshot.Value;
    }

    public void ChangeValue(int value) => this.ApplyEvent(new OtherEvent(value));

    public new void ApplyEvent(object @event) => base.ApplyEvent(@event);

    private void Apply(OtherEvent @event) => this.Value = @event.Value;
}