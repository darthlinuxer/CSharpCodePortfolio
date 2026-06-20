using System.Reflection;
using System.Runtime.ExceptionServices;
using EFCore10.Tutorials.Abstractions;

namespace EFCore10.App.Tutorials;

internal sealed record TutorialDescriptor(
    string Id,
    string Slug,
    string Title,
    Type TutorialType)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tutorial = CreateTutorial();
        await tutorial.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    private ITutorial CreateTutorial()
    {
        try
        {
            return Activator.CreateInstance(TutorialType) as ITutorial
                ?? throw new InvalidOperationException($"{TutorialType.FullName} must implement {nameof(ITutorial)}.");
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
