namespace EFCore10.Tutorials.Tutorial07.Models;

public readonly record struct ResourceId
{
    private ResourceId(Guid value) => Value = value;

    public Guid Value { get; }

    public static ResourceId NewId() => new(Guid.CreateVersion7());

    public static ResourceId From(Guid value) =>
        value == Guid.Empty ? throw new DomainException("Resource ID cannot be empty.") : new(value);

    public override string ToString() => Value.ToString();
}

public sealed record ResourceTitle
{
    public const int MaxLength = 120;

    private ResourceTitle(string value) => Value = value;

    public string Value { get; }

    public static ResourceTitle Create(string value) =>
        new(ValueObjectGuards.NormalizeLength(value, nameof(ResourceTitle), 5, MaxLength));

    public override string ToString() => Value;
}

public sealed record InstructorName
{
    public const int MaxLength = 80;

    private InstructorName(string value) => Value = value;

    public string Value { get; }

    public static InstructorName Create(string value) =>
        new(ValueObjectGuards.NormalizeLength(value, nameof(InstructorName), 3, MaxLength));

    public override string ToString() => Value;
}

public sealed record LearningLevel
{
    public static readonly LearningLevel Beginner = new("Beginner");
    public static readonly LearningLevel Intermediate = new("Intermediate");
    public static readonly LearningLevel Advanced = new("Advanced");

    private static readonly string[] AllowedValues = [Beginner.Value, Intermediate.Value, Advanced.Value];

    private LearningLevel(string value) => Value = value;

    public string Value { get; }

    public static LearningLevel Create(string value)
    {
        var normalized = ValueObjectGuards.NormalizeLength(value, nameof(LearningLevel), 1, 32);

        return AllowedValues.Contains(normalized, StringComparer.OrdinalIgnoreCase)
            ? new LearningLevel(AllowedValues.Single(candidate => candidate.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            : throw new DomainException($"Learning level must be one of: {string.Join(", ", AllowedValues)}.");
    }

    public override string ToString() => Value;
}

public readonly record struct WordCount
{
    private WordCount(int value) => Value = value;

    public int Value { get; }

    public static WordCount From(int value) => new(ValueObjectGuards.RequireAtLeast(value, 1, nameof(WordCount)));

    public override string ToString() => Value.ToString();
}

public readonly record struct VideoDuration
{
    private VideoDuration(int minutes) => Minutes = minutes;

    public int Minutes { get; }

    public static VideoDuration FromMinutes(int minutes) =>
        new(ValueObjectGuards.RequireAtLeast(minutes, 1, nameof(VideoDuration)));

    public override string ToString() => $"{Minutes} min";
}

public readonly record struct SeatLimit
{
    private SeatLimit(int value) => Value = value;

    public int Value { get; }

    public static SeatLimit From(int value) => new(ValueObjectGuards.RequireAtLeast(value, 1, nameof(SeatLimit)));

    public override string ToString() => Value.ToString();
}

internal static class ValueObjectGuards
{
    public static string NormalizeLength(string value, string label, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{label} is required.");

        var normalized = string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return normalized.Length >= minLength && normalized.Length <= maxLength
            ? normalized
            : throw new DomainException($"{label} must have between {minLength} and {maxLength} characters.");
    }

    public static int RequireAtLeast(int value, int minimum, string label) =>
        value >= minimum
            ? value
            : throw new DomainException($"{label} must be greater than or equal to {minimum}.");
}
