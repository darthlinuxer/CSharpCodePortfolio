namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record UniversityName
{
    private UniversityName(string value) => Value = value;

    public const int MaxLength = 160;

    public string Value { get; }

    internal static Result<UniversityName> Create(string? value)
    {
        var text = DomainText.Required(value, "University name", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<UniversityName>.Success(new UniversityName(text.RequireValue()))
            : Result<UniversityName>.Failure(text.Errors);
    }

    internal static Result<UniversityName> FromStorage(string value) => Create(value);
}
