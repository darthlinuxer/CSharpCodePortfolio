using Swashbuckle.AspNetCore.Filters;

namespace Swagger.ResponseExamples;

public class WeatherForecastExamples : IMultipleExamplesProvider<WeatherForecast>
{
    public IEnumerable<SwaggerExample<WeatherForecast>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Example 1",
            new WeatherForecast()
            {
                Date = DateTime.Now,
                TemperatureC = 40,
                Summary = "Hot"
            }
        );
         yield return SwaggerExample.Create(
            "Example 2",
            new WeatherForecast()
            {
                Date = DateTime.Now.AddDays(10),
                TemperatureC = 0,
                Summary = "Freezing"
            }
        );
    }
}