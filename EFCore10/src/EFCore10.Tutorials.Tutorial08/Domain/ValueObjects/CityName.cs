namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record CityName
{
    private CityName(string value) => Value = value;

    public const int MaxLength = 100;

    public string Value { get; }

    internal static Result<CityName> Create(string? value)
    {
        var text = DomainText.Required(value, "Campus city", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<CityName>.Success(new CityName(text.RequireValue()))
            : Result<CityName>.Failure(text.Errors);
    }

    internal static Result<CityName> FromStorage(string value) => Create(value);
}
