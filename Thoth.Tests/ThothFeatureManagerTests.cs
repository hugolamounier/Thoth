using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Thoth.Core;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Tests.Base;

namespace Thoth.Tests;

public class ThothFeatureManagerTests: IntegrationTestBase<Program>
{
    private static readonly Mock<ILogger<ThothFeatureManager>> Logger = new();
    private readonly IThothFeatureManager _thothFeatureManager;

    public ThothFeatureManagerTests() : base(serviceDelegate: services =>
    {
        services.AddScoped<ILogger<ThothFeatureManager>>(_ => Logger.Object);
    })
    {
        _thothFeatureManager = ServiceScope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
    }

    [Fact]
    public async Task IsEnabledAsync_WhenShouldReturnFalseWhenNotExists_ShouldSuccess()
    {
        //Arrange
        var flagName = Guid.NewGuid().ToString();

        //Act
        var result = await _thothFeatureManager.IsEnabledAsync(flagName);

        //Assert
        result.Should().BeFalse();
        Logger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!
                .Contains(string.Format(Messages.INFO_NON_EXISTENT_FLAG_REQUESTED, flagName))),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);

    }
}