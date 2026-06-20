using Microsoft.Extensions.Configuration;

namespace EFCore10.Shared;

/// <summary>
/// Holds a loaded tutorial configuration and the directory it was read from.
/// </summary>
public sealed record TutorialConfigurationResult(IConfiguration Configuration, string DirectoryPath);
