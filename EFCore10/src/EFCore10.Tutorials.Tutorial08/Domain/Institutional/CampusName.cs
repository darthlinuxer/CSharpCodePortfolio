namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed record CampusName
{
    private CampusName(string value) => Value = value;

    public const int MaxLength = 120;

    public string Value { get; }

    internal static Result<CampusName> Create(string? value)
    {
        var text = DomainText.Required(value, "Campus name", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<CampusName>.Success(new CampusName(text.RequireValue()))
            : Result<CampusName>.Failure(text.Errors);
    }

    internal static Result<CampusName> FromStorage(string value) => Create(value);
}
