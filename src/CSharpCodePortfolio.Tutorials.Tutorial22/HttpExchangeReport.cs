using System.Net;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed record HttpExchangeReport(
    HttpStatusCode StatusCode,
    EmployeeResponse? Employee,
    IReadOnlyList<string> Trace);
