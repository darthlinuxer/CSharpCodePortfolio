using Microsoft.Extensions.Logging;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class EmployeeQueryHandler(
    EmployeeDirectory directory,
    VerseClient verseClient,
    PipelineTrace trace,
    ILogger<EmployeeQueryHandler> logger)
{
    public async Task<EmployeeResponse?> HandleAsync(GetEmployeeQuery query, CancellationToken cancellationToken)
    {
        trace.Record("handler:consulta");
        logger.LogInformation("Consultando funcionário {EmployeeId}", query.Id);

        var employee = directory.FindById(query.Id);
        if (employee is null)
        {
            return null;
        }

        var verse = await verseClient.GetAsync(employee.FavoriteVerse, cancellationToken);
        return new EmployeeResponse(employee.Id, employee.Name, verse.Reference, verse.Text);
    }
}
