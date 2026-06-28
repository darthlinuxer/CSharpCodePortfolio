namespace EFCore10.Tutorials.Tutorial08.Domain.Institutional;

internal sealed record StaffRole
{
    private StaffRole(string value) => Value = value;

    public const int MaxLength = 80;

    public string Value { get; }

    internal static Result<StaffRole> Create(string? value)
    {
        var text = DomainText.Required(value, "Staff role", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<StaffRole>.Success(new StaffRole(text.RequireValue()))
            : Result<StaffRole>.Failure(text.Errors);
    }

    internal static Result<StaffRole> FromStorage(string value) => Create(value);
}
