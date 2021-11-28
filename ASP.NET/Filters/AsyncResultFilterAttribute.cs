using Microsoft.AspNetCore.Mvc.Filters;

//Resource Filters are useful to short-circuit most of the pipeline
//Example: A caching filter can avoid the rest of the pipeline on a cache hit

public class AsyncResultFilterAttribute : Attribute, IAsyncAlwaysRunResultFilter
{
    private readonly ILogger<AsyncResultFilterAttribute> logger;
    private readonly string name;
    public Guid Id { get; set; } = Guid.NewGuid();

    public AsyncResultFilterAttribute(
        ILogger<AsyncResultFilterAttribute> logger,
        string name
        )
    {
        this.logger = logger;
        this.name = name;
    }
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {            
        logger.LogInformation($"{Id} AsyncResultFilter executed on every request: {name}");
        await next(); 
    }
}