using CSharpCodePortfolio.App.Tutorials;
using Spectre.Console;

namespace CSharpCodePortfolio.App.Commands;

internal static class TutorialTable
{
    public static Table Create(IEnumerable<TutorialDescriptor> tutorials)
    {
        var table = new Table()
            .Title("CSharpCodePortfolio Tutorials")
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
