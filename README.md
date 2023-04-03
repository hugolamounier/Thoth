# <img src="./docs/icon.png" width="48" /> Thoth

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=hugolamounier_Thoth&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=hugolamounier_Thoth)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=hugolamounier_Thoth&metric=coverage)](https://sonarcloud.io/summary/new_code?id=hugolamounier_Thoth)

A .NET library to easily manage feature flags during runtime, without the need to deploy any new code.

# Features
- Runtime feature management, using database to store flags;
- In-memory feature flag caching
- Simple Dashboard to control features state

# Install

| Package name        | Nuget                                                                                                         |
|---------------------|---------------------------------------------------------------------------------------------------------------|
| Thoth.Core          | [![Nuget](https://img.shields.io/nuget/v/Thoth.Core)](https://www.nuget.org/packages/Thoth.Core/)             |
| Thoth.SQLServer     | [![Nuget](https://img.shields.io/nuget/v/Thoth.SQLServer)](https://www.nuget.org/packages/Thoth.SQLServer/)   |
| Thoth.Dashboard     | [![Nuget](https://img.shields.io/nuget/v/Thoth.Dashboard)](https://www.nuget.org/packages/Thoth.Dashboard/)   |

# Usage

Add the Thoth Feature Manager to yout application DI (Dependency Injection) container:

```c#
builder.Services.AddThoth(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("SqlContext"); // Your application sql database connection
    options.EnableCaching = true; // Whether Thoth should use caching strategies to improve performance. Optional.
    options.CacheExpiration = TimeSpan.FromDays(7); // Defines for how long feature flags are going to be cached in memory. Optional.
    options.CacheSlidingExpiration = TimeSpan.FromDays(1); // Defines for how long the feature flags will be cached without being accessed. Optional.
    options.EnableThothApi = True; // Defines if the Thoth Api should be exposed. This is required true when using Dashboard.
}).UseSqlServer(); // Sets Thoth to use SQLServer as its database storage.
```

A Dashboard can optioanally be injected to the application:
```c#
var app = builder.Build();
app.UseThothDashboard(options =>
{
    options.RoutePrefix = "/thoth"; // Defines the route prefix to access the dashboard. Optional.
});
```

Runtime feature management as simples as:

```c#
public class MyService
{
    private readonly IThothFeatureManager _thothFeatureManager;

    public MyService(IThothFeatureManager thothFeatureManager)
    {
        _thothFeatureManager = thothFeatureManager;
    }

    public async Task<bool> MyServiceMethod()
    {
        var isFeatureEnabled = await _thothFeatureManager.IsEnabledAsync("MyFeatureFlagName");
        
        if(isFeatureEnabled)
        {
            // do something
        }
        
    }
}
```

A [Sample project](https://github.com/hugolamounier/Thoth/tree/master/Thoth.Sample) is available to DEMO the library.
