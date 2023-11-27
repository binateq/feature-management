using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Demo.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    };

    private readonly IFeatureManager _featureManager;

    public WeatherForecastController(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    [HttpGet]
    [Produces(typeof(IEnumerable<WeatherForecast>))]
    public async Task<IActionResult> Get()
    {
        if (!await _featureManager.IsEnabledAsync("weather-forecast"))
            return NotFound();

        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
                            .ToArray());
    }

    [HttpGet("flipt")]
    [Produces(typeof(IEnumerable<WeatherForecast>))]
    public async Task<IActionResult> GetFlipt()
    {
        if (!await _featureManager.IsEnabledAsync("weather-forecast-flipt"))
            return NotFound();

        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
                            .ToArray());
    }

    [HttpGet("principal")]
    [Produces(typeof(IEnumerable<WeatherForecast>))]
    [Authorize]
    public async Task<IActionResult> GetPrincipal()
    {
        if (!await _featureManager.IsEnabledAsync("weather-forecast-principal", HttpContext.User))
            return NotFound();

        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
                            .ToArray());
    }
}
