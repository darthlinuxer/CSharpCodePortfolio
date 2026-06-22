namespace EFCore10.Tutorials.Tutorial07.Models;

public sealed class DomainException(string message) : Exception(message);
