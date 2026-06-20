using EFCore10.App.Tutorials;
using Spectre.Console;

namespace EFCore10.App.Commands;

internal static class TutorialTable
{
    public static Table Create(IEnumerable<TutorialDescriptor> tutorials)
    {
        var table = new Table()
            .Title("EF Core 10 Tutorials")
            .AddColumn("Id")
            .AddColumn("Slug")
            .AddColumn("Title");

        foreach (var tutorial in tutorials)
        {
            table.AddRow(
                Markup.Escape(tutorial.Id),
                Markup.Escape(tutorial.Slug),
                Markup.Escape(tutorial.Title));
        }

        return table;
    }
}
