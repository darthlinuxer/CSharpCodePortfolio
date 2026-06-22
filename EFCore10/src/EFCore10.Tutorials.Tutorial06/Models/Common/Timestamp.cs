namespace EFCore10.Tutorials.Tutorial06.Models;

public readonly record struct Timestamp
{
    private Timestamp(DateTime value) => Value = value.Kind == DateTimeKind.Utc
        ? value
        : throw new ArgumentException("Timestamp must be in UTC.", nameof(value));

    public DateTime Value { get; }

    public static Timestamp UtcNow =>
        new(DateTime.UtcNow);

    public static Timestamp FromUtc(DateTime value) => new(value);

    public static Timestamp FromDatabase(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => new Timestamp(value),
        DateTimeKind.Unspecified => new Timestamp(DateTime.SpecifyKind(value, DateTimeKind.Utc)),
        _ => throw new ArgumentException("Timestamp from database must be UTC or unspecified.", nameof(value))
    };

    public Timestamp Add(TimeSpan timeSpan) =>
        new(Value.Add(timeSpan));

    public static bool operator >(Timestamp left, Timestamp right) => left.Value > right.Value;
    public static bool operator >=(Timestamp left, Timestamp right) => left.Value >= right.Value;
    public static bool operator <(Timestamp left, Timestamp right) => left.Value < right.Value;
    public static bool operator <=(Timestamp left, Timestamp right) => left.Value <= right.Value;

    public override string ToString() => Value.ToString("O");
}
