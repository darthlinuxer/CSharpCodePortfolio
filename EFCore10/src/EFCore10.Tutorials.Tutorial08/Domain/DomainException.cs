namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
