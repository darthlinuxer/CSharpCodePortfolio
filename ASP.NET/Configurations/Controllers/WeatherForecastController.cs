using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Configurations.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration config;
    private readonly IOptions<MyApiOptions> _options;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IConfiguration config, 
        IOptions<MyApiOptions> options)
    {
        _logger = logger;
        this.config = config;
        this._options = options;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation($"MyKey : {config["MyKey"]}");
        _logger.LogInformation($"MyApiOptions URL: {_options.Value.URL}");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
