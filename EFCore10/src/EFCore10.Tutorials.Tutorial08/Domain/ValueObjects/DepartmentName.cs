namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record DepartmentName
{
    private DepartmentName(string value) => Value = value;

    public const int MaxLength = 120;

    public string Value { get; }

    internal static Result<DepartmentName> Create(string? value)
    {
        var text = DomainText.Required(value, "Department name", minLength: 3, MaxLength);

        return text.IsSuccess
            ? Result<DepartmentName>.Success(new DepartmentName(text.RequireValue()))
            : Result<DepartmentName>.Failure(text.Errors);
    }

    internal static Result<DepartmentName> FromStorage(string value) => Create(value);
}
