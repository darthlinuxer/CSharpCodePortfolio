using System.Net;

namespace CSharpCodePortfolio.Tutorials.Tutorial20;

internal sealed record PingResult(HttpStatusCode StatusCode, string Body, bool IsSuccess);
