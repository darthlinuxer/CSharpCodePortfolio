namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed record AspNetCorePipelineReport(
    string BaseAddress,
    string ApiUrl,
    string ApiKey,
    int RetryCount,
    string EndpointName,
    IReadOnlyList<string> EndpointTags,
    HttpExchangeReport EmployeeRequest,
    HttpExchangeReport CachedRequest);
