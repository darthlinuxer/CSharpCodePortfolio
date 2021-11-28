using Microsoft.AspNetCore.Mvc;

namespace Filters.Controllers;

[ApiController]
[Route("[controller]")]
[SyncActionFilter("Controller")]
[AsyncResourceFilter("Controller")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [SyncActionFilter("Action")]
    [AsyncActionFilter("Action Executed First: Order is -1", -1)]
    [AsyncResourceFilter("Action")]
    [TypeFilter(typeof(AsyncResultFilterAttribute), Arguments = new object[]{"Action"})]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet]
    [Route("Error")]
    [ServiceFilter(typeof(AsyncExceptionFilterAttribute))]
    public IActionResult ThrowError()
    {
        throw new Exception("Exception created withing ThrowError Endpoint!");
    }
}
