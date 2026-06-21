namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class DomainException(string message) : Exception(message);
