namespace SimpleDomain.TestDoubles;

public class MyDynamicEventSourcedAggregateRoot : DynamicEventSourcedAggregateRoot
{
    public MyDynamicEventSourcedAggregateRoot() { }

    public MyDynamicEventSourcedAggregateRoot(string aggregateId) => this.Id = aggregateId;

    public int Value { get; private set; }

    public override ISnapshot CreateSnapshot() => new MySnapshot(this.Value, this.Version, DateTimeOffset.UtcNow);

    public override void LoadFromSnapshot(ISnapshot snapshot)
    {
        if (snapshot is not MySnapshot mySnapshot) return;

        this.Version = mySnapshot.Version;
        this.Value = mySnapshot.Value;
    }

    public void ChangeValue(int value) => this.ApplyEvent(new ValueEvent(value));

    public new void ApplyEvent(IEvent @event) => base.ApplyEvent(@event);

    private void Apply(ValueEvent @event) => this.Value = @event.Value;
}

public class MyStaticEventSourcedAggregateRoot : StaticEventSourcedAggregateRoot
{
    public MyStaticEventSourcedAggregateRoot() => this.RegisterTransition<ValueEvent>(this.Apply);

    public MyStaticEventSourcedAggregateRoot(string aggregateId) : this() => this.Id = aggregateId;

    public int Value { get; private set; }

    public override ISnapshot CreateSnapshot() => new MySnapshot(this.Value, this.Version, DateTimeOffset.UtcNow);

    public override void LoadFromSnapshot(ISnapshot snapshot)
    {
        if (snapshot is not MySnapshot mySnapshot) return;

        this.Version = mySnapshot.Version;
        this.Value = mySnapshot.Value;
    }

    public void ChangeValue(int value) => this.ApplyEvent(new ValueEvent(value));

    public new void ApplyEvent(IEvent @event) => base.ApplyEvent(@event);

    private void Apply(ValueEvent @event) => this.Value = @event.Value;
}