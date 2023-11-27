# Binateq.FeatureManagement.Flipt

The NuGet package **Binateq.FeatureManagement.Flipt** allows to use the feature-flagging solution
[**Flipt**](https://www.flipt.io/) together with the
[**Microsoft.FeatureManagement**](https://www.nuget.org/packages/Microsoft.FeatureManagement/) package.

## What for?

The **Microsoft.FeatureManagement** provides simple and portable standard for .NET applications. It can be
extended with *feature filters*, so you can use both static and dynamic feature flags.

You can add the packet
[**Microsoft.Azure.AppConfiguration.AspNetCore**](https://www.nuget.org/packages/Microsoft.Azure.AppConfiguration.AspNetCore)
to use dynamic feature flags configured in __Microsoft Azure App Configuration service__.

But what can you do, if you use *Docker/Kubernetes* instead of *Azure*? You can use any feature toggle solution like
**Flipt** that has its own unique methods to work with feature flags.

Or you can use this package that implements feature filters' interface for **Flipt** gRPC API. In the latter case you
can move your application from *Azure* to *Docker* and vice versa.

## Quick Start

We'll use the standard
[ASP.NET Core template with Weather Forecast API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api)
to check feature flags.

Make the demo Web API application, as it described in the article above. Choose the type **Web API** and
**Windows Authentication**.

Run it and check, if it works and accessible through the Swagger.

### Classic Microsoft.FeatureManagement

Install the **Microsoft.FeatureManagement** packet:

```shell
dotnet add package Microsoft.FeatureManagement --version 2.6.1
```

Please note that you need to install the version **2.6.1** because something was broken in the **3.0.0.**.

We all are waiting for updates.

Include feature management support to configuration:

```c#
builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureFlags"));
```

Append new feature flag **weather-forecast** to the **application.json** configuration file:

```json
{
  "FeatureFlags": {
    "weather-forecast": false
  }
}
```

Append the `_featureManager` to 
[`WeatherForecastController`](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/tutorials/first-web-api/samples/3.0/TodoApi/Controllers/WeatherForecastController.cs)
and initialize it through the constructor:

```c#
private readonly ILogger<WeatherForecastController> _logger;
private readonly IFeatureManager _featureManager;

public WeatherForecastController(ILogger<WeatherForecastController> logger, IFeatureManager featureManager)
{
    _logger = logger;
    _featureManager = featureManager;
}
```

Rewrite the `Get` method to use the feature flag:

```c#
[HttpGet]
[Produces(typeof(IEnumerable<WeatherForecast>))]
public async Task<IActionResult> Get()
{
    if (await _featureManager.IsEnabledAsync("weather-forecast"))
    {
        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            }));
    }

    return NotFound();
}
```

Run the application and call the `Get` though the Swagger. It should return 404 status.
Then make file enabled (in the **application.json**) and re-run the application.
Call the `Get` again. IT should return 200 status and the collection of forecast.

This static flag requires re-running of re-deploying of applications. Now let's try to use the dynamic flag.

### Dynamic flag from Flipt

Install and run **Flipt** Docker image:

```shell
docker run -d -p 8080:8080 -p 9000:9000 -v $HOME/flipt:/var/opt/flipt docker.flipt.io/flipt/flipt:latest
```

8080 is the port of **Flipt** Web UI and REST API, and 9000 is the port of gRPC API.

Open [http://localhost:8080](http://localhost:8080) after running. Enter the **Flags** panel and create
**Boolean** flag with name **Weather Forecast** and key **weather-forecast**.

![Creating feature flag in Flipt UI](https://github.com/binateq/feature-management/assets/10639110/e45094a0-64a6-4ba6-a23f-7c1661e279f0)

Install the package **Binateq.FeatureManagement.Flipt**:

```shell
dotnet add package Binateq.FeatureManagement.Flipt
```

Append `FliptFeatureFilter` to *composition root*:

```c#
builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureFlags"))
       .AddFeatureFilter<FliptFeatureFilter>();
```

Set gRPC API URL and `FliptFeatureFilter` for the **weather-forecast** flag in the **application.json**:

```json
{
  "Flipt": {
    "Url": "http://localhost:9000"
  },
  "FeatureFlags": {
    "weather-forecast": {
      "EnabledFor": [
        {
          "Name": "FliptFeature"
        }
      ]
    }
  }
}
```

Please note that you need specify the filter `FliptFeature` in the configuration, although the class is called
`FliptFeatureFilter`.

Run the application and call the `Get` though the Swagger. It should return 404 status.
*Enable* the flag **weather-forecast** in the **Flipt** Web UI and repeat the call.
The application should return 200 status and a list of forecasts.

### User-specific flag form Flipt

Append `FliptPrincipalFeatureFilter` to *composition root*:

```c#
builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureFlags"))
       .AddFeatureFilter<FliptFeatureFilter>()
       .AddFeatureFilter<FliptPrincipalFeatureFilter>();
```

Append `UserIdClaim` to the `Flipt` configuration section. Append **weather-forecast-principal** also.

```json
{
  "Flipt": {
    "Url": "http://localhost:9000",
    "UserIdClaim": "ClaimTypes.PrimarySid"
  },
  "FeatureFlags": {
    "weather-forecast": {
      "EnabledFor": [
        {
          "Name": "FliptFeature"
        }
      ]
    },
    "weather-forecast-principal": {
      "EnabledFor": [
        {
          "Name": "FliptPrincipal"
        }
      ]
    }
  }
}
```

Append new `GetPrincipal` method to use the feature flag:

```c#
[HttpGet("principal")]
[Produces(typeof(IEnumerable<WeatherForecast>))]
[Authorize]
public async Task<IActionResult> GetPrincipal()
{
    if (await _featureManager.IsEnabledAsync("weather-forecast-forecast", HttpContext.User))
    {
        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            }));
    }

    return NotFound();
}
```

Run the **Demo** application and check **GET /weather-forecast/principal** endpoint. It should return **Not Found**.
Now run in command line:

```shell
whoami /user
```

You'll see SID of your user account. Open **Flipt UI**, and create new segment **Specified Users** with the key
**specified-users**. Set **Match Type** to **Any**.

Append the constraint **UserId** with your account's SID as the value. Choose **==** as the **operator**.

Create new **Boolean** flag **Weather Forecast Principal** with the key **weather-forecast-principal**. Append
the **Rollout** of the **Segment** type. Choose the segment **specified-users**, and set the **Value** field to `true`.

Check **GET /weather-forecast/principal** endpoint again. It should return a list of forecasts and **Ok** status.
