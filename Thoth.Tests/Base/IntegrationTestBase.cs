using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Thoth.Core.Models;

namespace Thoth.Tests.Base;

[Collection("Integration Test Collection")]
public abstract class IntegrationTestBase<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IDisposable where TEntryPoint : class
{
    protected HttpClient HttpClient { get; private set; } = null!;
    protected IServiceScope ServiceScope { get; private set; } = null!;
    private readonly Action<IServiceCollection>? _serviceCollectionOverride;
    private readonly Dictionary<string, string>? _arguments;

    protected IntegrationTestBase(Action<IServiceCollection>? serviceDelegate = null, Dictionary<string, string>? arguments = null)
    {
        _arguments = arguments;
        _serviceCollectionOverride = serviceDelegate;
        ConfigureServer();
    }
    
    protected void ConfigureServer()
    {
        HttpClient = CreateClient();
        ServiceScope = Services.CreateScope();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
        
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(c =>
        {
            c.AddConfiguration(configuration);
        });

        if (_arguments?.Any() ?? false)
            builder.ConfigureWebHost(x =>
            {
                foreach (var arg in _arguments)
                {
                    x.UseSetting(arg.Key, arg.Value);
                }
            });
        
        if(_serviceCollectionOverride is not null)
            builder.ConfigureServices(_serviceCollectionOverride);

        return base.CreateHost(builder);
    }

    protected virtual void AfterEachTestAsync()
    {
        ServiceScope.ServiceProvider.GetRequiredService<IOptions<ThothOptions>>().Value.DatabaseProvider.Value.Dispose();
        HttpClient.Dispose();
        ServiceScope.Dispose();
    }

    public new void Dispose()
    {
        AfterEachTestAsync();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}