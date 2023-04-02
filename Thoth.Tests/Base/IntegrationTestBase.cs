using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Thoth.Tests.Base;

public abstract class IntegrationTestBase<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    protected HttpClient HttpClient;
    protected IServiceScope ServiceScope;
    private readonly Action<IServiceCollection>? _serviceCollectionOverride; 
    
    protected IntegrationTestBase(Action<IServiceCollection>? serviceDelegate = null)
    {
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
        
        if(_serviceCollectionOverride is not null)
            builder.ConfigureServices(_serviceCollectionOverride);

        return base.CreateHost(builder);
    }

    private void AfterEachTestAsync()
    {
        HttpClient.Dispose();
        ServiceScope.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        AfterEachTestAsync();
        GC.SuppressFinalize(this);
        return base.DisposeAsync();
    }
}