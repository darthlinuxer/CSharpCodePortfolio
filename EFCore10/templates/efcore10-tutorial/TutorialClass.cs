using EFCore10.Shared;
using EFCore10.Tutorials.Abstractions;

namespace EFCore10.Tutorials.TutorialTemplate;

[Tutorial("TutorialId", "TutorialSlug", "TutorialTitle")]
public sealed class TutorialClass : ITutorial
{
    private const string ConnectionStringName = "TutorialDatabase";

    public Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorialConfiguration = TutorialConfiguration.LoadForAssembly(typeof(TutorialClass).Assembly);
        var connectionString = SqliteConnectionStrings.GetRequired(
            tutorialConfiguration.Configuration,
            ConnectionStringName,
            tutorialConfiguration.DirectoryPath);

        Console.WriteLine("TutorialId - TutorialTitle");
        Console.WriteLine($"SQLite connection string: {connectionString}");

        return Task.CompletedTask;
    }
}
