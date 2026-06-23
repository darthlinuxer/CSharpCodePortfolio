using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class EndpointTraceFilter(PipelineTrace trace) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        trace.Record("filter:endpoint:antes");
        var result = await next(context);
        trace.Record("filter:endpoint:depois");
        return result;
    }
}
