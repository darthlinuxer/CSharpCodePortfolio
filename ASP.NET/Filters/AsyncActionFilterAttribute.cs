using Microsoft.AspNetCore.Mvc.Filters;

public class AsyncActionFilterAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    private readonly string name;
    private int count = 0;
    public Guid Id { get; set; } = Guid.NewGuid();

    public AsyncActionFilterAttribute(string name, int order = 0)
    {
        this.name = name;
        this.Order = order;
    }

    public int Order {get; set;}

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        Console.WriteLine($"{Id} AsyncActionFilterAtribute: Before request => {name} {count}");
        await next();
        Console.WriteLine($"{Id} AsyncActionFilterAtribute: After request => {name} {count}");
        count ++;
    }

}