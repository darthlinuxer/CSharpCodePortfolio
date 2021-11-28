public class ConsoleLoggerMiddleWare : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Console.WriteLine("Before Request: Logger Middleware");
        await next(context);
        Console.WriteLine("After Request: Logger Middleware");
    }
}