using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//Exception Filters are good for trapping exceptions that occur within actions
// but are not as flexible as error handling middleware
// One shoud prefer error handling middleware

public class AsyncExceptionFilterAttribute : Attribute, IAsyncExceptionFilter
{
    private readonly ILogger<AsyncExceptionFilterAttribute> logger;
    private readonly IWebHostEnvironment hostingEnv;

    public AsyncExceptionFilterAttribute(
        ILogger<AsyncExceptionFilterAttribute> logger,
        IWebHostEnvironment hostingEnv)
    {
        this.logger = logger;
        this.hostingEnv = hostingEnv;
    }
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        logger.LogInformation($"ExceptionFilter called: {context.Exception.Message}");
        if(!hostingEnv.IsDevelopment()) return;
        context.Result = new ContentResult()
        {
            ContentType = "text",
            StatusCode = 500,
            Content = "Custom Exception Filter:" + context.Exception.Message
        };
    }
}