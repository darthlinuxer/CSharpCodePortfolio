using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//Resource Filters are useful to short-circuit most of the pipeline
//Example: A caching filter can avoid the rest of the pipeline on a cache hit

public class AsyncResourceFilterAttribute : Attribute, IAsyncResourceFilter
{
    private readonly string name;
    public Guid Id { get; set; } = Guid.NewGuid();

    public AsyncResourceFilterAttribute(string name)
    {
        this.name = name;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
         if(context.HttpContext.Request.Query["cache"].Any())
            context.Result = new ContentResult()
            {
                StatusCode = 200,
                ContentType = "text",
                Content = "This is a cached response"
            };    
            
        Console.WriteLine($"{Id} AsyncResourceFilter Before Request: {name}");
        await next(); 
        Console.WriteLine($"{Id} AsyncResourceFilter After Request: {name}");
    }

    
}