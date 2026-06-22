namespace EFCore10.Tutorials.Tutorial07.Models;

public abstract class LearningResource
{
    protected LearningResource()
    {
    }

    protected LearningResource(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        DateTime createdOnUtc)
    {
        Id = ResourceId.NewId();
        Title = title;
        Instructor = instructor;
        Level = level;
        CreatedOnUtc = RequireUtc(createdOnUtc);
    }

    public ResourceId Id { get; private set; }

    public ResourceTitle Title { get; private set; } = null!;

    public InstructorName Instructor { get; private set; } = null!;

    public LearningLevel Level { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? PublishedOnUtc { get; private set; }

    public bool IsPublished => PublishedOnUtc is not null;

    public string ResourceKind => GetType().Name.Replace(nameof(LearningResource), string.Empty, StringComparison.Ordinal);

    public void Rename(ResourceTitle title)
    {
        ArgumentNullException.ThrowIfNull(title);
        Title = title;
    }

    public void Publish(DateTime occurredOnUtc)
    {
        if (IsPublished)
            throw new DomainException($"{ResourceKind} '{Title}' has already been published.");

        EnsureCanPublish();
        PublishedOnUtc = RequireUtc(occurredOnUtc);
    }

    public void EnsureCanPublish() => EnsureSpecificPublicationRules();

    public abstract string FormatSpecifics();

    protected abstract void EnsureSpecificPublicationRules();

    private static DateTime RequireUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : throw new DomainException("Domain timestamps must be UTC.");
}

public sealed class ArticleResource : LearningResource
{
    private const int MinimumPublishableWords = 600;

    private ArticleResource()
    {
    }

    private ArticleResource(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        WordCount wordCount,
        DateTime createdOnUtc)
        : base(title, instructor, level, createdOnUtc)
    {
        WordCount = wordCount;
    }

    public WordCount WordCount { get; private set; }

    public static ArticleResource Create(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        WordCount wordCount,
        DateTime createdOnUtc) =>
        new(title, instructor, level, wordCount, createdOnUtc);

    public override string FormatSpecifics() => $"{WordCount.Value:N0} words";

    protected override void EnsureSpecificPublicationRules()
    {
        if (WordCount.Value < MinimumPublishableWords)
            throw new DomainException($"Articles need at least {MinimumPublishableWords} words to be published.");
    }
}

public sealed class VideoResource : LearningResource
{
    private const int MinimumPublishableMinutes = 5;

    private VideoResource()
    {
    }

    private VideoResource(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        VideoDuration duration,
        DateTime createdOnUtc)
        : base(title, instructor, level, createdOnUtc)
    {
        Duration = duration;
    }

    public VideoDuration Duration { get; private set; }

    public static VideoResource Create(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        VideoDuration duration,
        DateTime createdOnUtc) =>
        new(title, instructor, level, duration, createdOnUtc);

    public override string FormatSpecifics() => Duration.ToString();

    protected override void EnsureSpecificPublicationRules()
    {
        if (Duration.Minutes < MinimumPublishableMinutes)
            throw new DomainException($"Videos need at least {MinimumPublishableMinutes} minutes to be published.");
    }
}

public sealed class LiveWorkshopResource : LearningResource
{
    private const int MinimumPublishableSeats = 5;

    private LiveWorkshopResource()
    {
    }

    private LiveWorkshopResource(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        SeatLimit seatLimit,
        DateTime createdOnUtc)
        : base(title, instructor, level, createdOnUtc)
    {
        SeatLimit = seatLimit;
    }

    public SeatLimit SeatLimit { get; private set; }

    public static LiveWorkshopResource Create(
        ResourceTitle title,
        InstructorName instructor,
        LearningLevel level,
        SeatLimit seatLimit,
        DateTime createdOnUtc) =>
        new(title, instructor, level, seatLimit, createdOnUtc);

    public override string FormatSpecifics() => $"{SeatLimit.Value} seats";

    protected override void EnsureSpecificPublicationRules()
    {
        if (SeatLimit.Value < MinimumPublishableSeats)
            throw new DomainException($"Live workshops need at least {MinimumPublishableSeats} seats to be published.");
    }
}
