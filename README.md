# <img width="64" src="./docs/icon.png" alt="Thoth logo"> Thoth

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=hugolamounier_Thoth&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=hugolamounier_Thoth)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=hugolamounier_Thoth&metric=coverage)](https://sonarcloud.io/summary/new_code?id=hugolamounier_Thoth)

Thoth is a lightweight .NET library that provides runtime feature management functionality for .NET applications. It allows developers to easily manage and enable/disable features in their applications without requiring a rebuild or redeployment of the application. Thoth is designed to be flexible and extensible, and can work with any database provider using the `IDatabase` interface.

## Projects

Thoth consists of the following projects:

### Thoth.Core

`Thoth.Core` is responsible for providing the core feature management functionality. It includes the `IThothFeatureManager` interface, which defines the methods for managing features, and the `ThothFeatureManager` class, which implements this interface. The `ThothFeatureManager` class can work with any database provider that implements the `IDatabase` interface.

### Thoth.Dashboard

`Thoth.Dashboard` is a web-based dashboard created using React that provides an easy-to-use interface for managing features. The dashboard allows users to enable/disable features, view feature status, and more.

### Thoth.SQLServer

`Thoth.SQLServer` is a database provider for SQL Server that implements the `IDatabase` interface. It provides a simple and easy-to-use implementation for storing and retrieving feature data from a SQL Server database.

## Installation

| Package name        | Nuget                                                                                                         |
|---------------------|---------------------------------------------------------------------------------------------------------------|
| Thoth.Core          | [![Nuget](https://img.shields.io/nuget/v/Thoth.Core)](https://www.nuget.org/packages/Thoth.Core/)             |
| Thoth.SQLServer     | [![Nuget](https://img.shields.io/nuget/v/Thoth.SQLServer)](https://www.nuget.org/packages/Thoth.SQLServer/)   |
| Thoth.Dashboard     | [![Nuget](https://img.shields.io/nuget/v/Thoth.Dashboard)](https://www.nuget.org/packages/Thoth.Dashboard/)   |


## Getting Started
To start using Thoth in your .NET application, follow these steps:

```c#
using Thoth.Core;
using Thoth.SQLServer;

builder.Services.AddThoth(options =>
{
    options.DatabaseProvider = new ThothSqlServerProvider(builder.Configuration.GetConnectionString("SqlContext")) ; // Set database provider
    options.ShouldReturnFalseWhenNotExists = true; // Defines if the default value to a non-existent should be false or throw
    options.EnableCaching = true; // Whether Thoth should use caching strategies to improve performance. Optional.
    options.CacheExpiration = TimeSpan.FromDays(7); // Defines for how long feature flags are going to be cached in memory. Optional.
    options.CacheSlidingExpiration = TimeSpan.FromDays(1); // Defines for how long the feature flags will be cached without being accessed. Optional.
    options.EnableThothApi = True; // Defines if the Thoth Api should be exposed. This is required true when using Dashboard.
});
```

The Dashboard can optionally be injected to the application:
```c#
using Thoth.Dashboard;

var app = builder.Build();
app.UseThothDashboard(options =>
{
    options.RoutePrefix = "/thoth"; // Defines the route prefix to access the dashboard. Optional.
});
```

### Usage

```c#
public class MyClass
{
    private readonly IThothFeatureManager _thothFeatureManager;

    public MyClass(IThothFeatureManager thothFeatureManager)
    {
        _thothFeatureManager = thothFeatureManager;
    }
    
    public async void MyMethod()
    {
        // Gets the value of an environment variable and converts to de desired type.
        // The environment variable value is saved as string on the database.
        var myEnvVariable = await _thothFeatureManager.GetEnvironmentValueAsync<int>("MyFeatureName");
        
        // some code
    }
    
    public async void Set()
    {
        // Gets if the feature is enabled.
        var isEnabled = await _thothFeatureManager.IsEnabledAsync("MyFeatureFlagName");
        
        if(isEnabled)
        {
            // do something
        }
    }
}
```

## Demo
A [Sample project](https://github.com/hugolamounier/Thoth/tree/master/Thoth.Sample) is available to DEMO the library.

## License
Thoth is released under the MIT License. See the LICENSE file for details.