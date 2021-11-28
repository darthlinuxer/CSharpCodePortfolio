using Microsoft.AspNetCore.Mvc.Filters;

public class SyncActionFilterAttribute : Attribute, IActionFilter
{
    private readonly string name;
    public Guid Id { get; set; } = Guid.NewGuid();

    public SyncActionFilterAttribute(string name)
    {
        this.name = name;
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
        Console.WriteLine($"{Id} SyncActionFilterAtribute: After request => {name}");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        Console.WriteLine($"{Id} SyncActionFilterAtribute: Before request => {name}");
    }
}