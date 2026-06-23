using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class RequestTraceMiddleware(PipelineTrace trace) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        trace.Record("middleware:classe:antes");
        context.Response.Headers["X-Portfolio-Pipeline"] = "visited";
        await next(context);
        trace.Record("middleware:classe:depois");
    }
}
