using System.Reflection;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.App.Tutorials;

internal static class TutorialRegistry
{
    private const string TutorialAssemblyPattern = "CSharpCodePortfolio.Tutorials.*.dll";

    public static IReadOnlyList<TutorialDescriptor> Discover()
    {
        var descriptors = new[] { ExternalTutorials.EfCore10() }
            .Concat(DiscoverLocalTutorials())
            .OrderBy(tutorial => tutorial.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        EnsureUnique(descriptors, tutorial => tutorial.Id, "id");
        EnsureUnique(descriptors, tutorial => tutorial.Slug, "slug");

        return descriptors;
    }

    private static IEnumerable<TutorialDescriptor> DiscoverLocalTutorials()
    {
        return Directory
            .EnumerateFiles(AppContext.BaseDirectory, TutorialAssemblyPattern, SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(LoadAssembly)
            .SelectMany(GetLoadableTypes)
            .Where(HasTutorialAttribute)
            .Select(CreateDescriptor);
    }

    private static Assembly LoadAssembly(string assemblyPath)
    {
        try
        {
            return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception exception) when (exception is BadImageFormatException or FileLoadException or FileNotFoundException)
        {
            throw new InvalidOperationException($"Could not load tutorial assembly '{assemblyPath}'.", exception);
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static bool HasTutorialAttribute(Type type)
    {
        return type is { IsClass: true, ContainsGenericParameters: false }
            && type.GetCustomAttribute<TutorialAttribute>() is not null;
    }

    private static TutorialDescriptor CreateDescriptor(Type tutorialType)
    {
        var metadata = tutorialType.GetCustomAttribute<TutorialAttribute>()
            ?? throw new InvalidOperationException($"{tutorialType.FullName} must be decorated with {nameof(TutorialAttribute)}.");

        if (tutorialType.IsAbstract || !typeof(ITutorial).IsAssignableFrom(tutorialType))
        {
            throw new InvalidOperationException($"{tutorialType.FullName} must be a non-abstract class that implements {nameof(ITutorial)}.");
        }

        if (tutorialType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new InvalidOperationException($"{tutorialType.FullName} must expose a public parameterless constructor.");
        }

        return new TutorialDescriptor(
            metadata.Id,
            metadata.Slug,
            metadata.Title,
            async cancellationToken =>
            {
                var tutorial = (ITutorial?)Activator.CreateInstance(tutorialType)
                    ?? throw new InvalidOperationException($"{tutorialType.FullName} could not be instantiated.");

                await tutorial.RunAsync(cancellationToken).ConfigureAwait(false);
            });
    }

    private static void EnsureUnique(
        IReadOnlyCollection<TutorialDescriptor> descriptors,
        Func<TutorialDescriptor, string> valueSelector,
        string fieldName)
    {
        var duplicate = descriptors
            .GroupBy(valueSelector, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Tutorial {fieldName} '{duplicate.Key}' is duplicated.");
        }
    }
}
