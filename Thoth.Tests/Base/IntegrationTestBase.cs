using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thoth.Sample.Contexts;

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

        var context = ServiceScope.ServiceProvider.GetRequiredService<SqlContext>();
        context.Database.Migrate();
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