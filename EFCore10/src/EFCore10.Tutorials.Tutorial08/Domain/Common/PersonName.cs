namespace EFCore10.Tutorials.Tutorial08.Domain.Common;

internal sealed record PersonName
{
    private PersonName(string value) => Value = value;

    public const int MaxLength = 160;

    public string Value { get; }

    internal static Result<PersonName> Create(string? value)
    {
        var text = DomainText.Required(value, "Person name", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<PersonName>.Success(new PersonName(text.RequireValue()))
            : Result<PersonName>.Failure(text.Errors);
    }

    internal static Result<PersonName> FromStorage(string value) => Create(value);
}
