namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PasswordHash
{
    public PasswordHash(string value) => Value = value;

    public string Value
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Password hash is required.");

            field = value.Length <= 500 ? value : throw new DomainException("Password hash must have at most 500 characters.");
        }
    }

    public static PasswordHash FromHash(string value) => new(value);

    public override string ToString() => Value;
}
