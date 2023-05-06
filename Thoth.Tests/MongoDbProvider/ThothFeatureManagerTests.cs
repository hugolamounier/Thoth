using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Thoth.Core;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;
using Thoth.Tests.Base;

namespace Thoth.Tests.MongoDbProvider;

public class ThothFeatureManagerTests : IntegrationTestBase<Program>
{
    private static readonly Mock<ILogger<ThothFeatureManager>> Logger = new();
    private readonly IThothFeatureManager _thothFeatureManager;

    public ThothFeatureManagerTests() : base(arguments: new Dictionary<string, string>
    {
        { "provider", "MongoDbProvider" }
    }, serviceDelegate: services => { services.AddScoped<ILogger<ThothFeatureManager>>(_ => Logger.Object); })
    {
        _thothFeatureManager = ServiceScope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
    }

    [Fact]
    public async Task IsEnabledAsync_ShouldReturnFalseWhenNotExists_ShouldSuccess()
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

    [Fact]
    public async Task GetEnvironmentValueAsync_ShouldSuccess()
    {
        //Arrange
        var featureManager = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.EnvironmentVariable,
            Description = "Test",
            Enabled = true,
            Value = "101"
        };

        //Act
        await _thothFeatureManager.AddAsync(featureManager);
        var feature = await _thothFeatureManager.GetEnvironmentValueAsync<int>(featureManager.Name);

        //Assert
        feature.Should().BeOfType(typeof(int));
        feature.Should().Be(int.Parse(featureManager.Value));
    }

    [Fact]
    public async Task GetEnvironmentValueAsync_WhenTypeNotValid_ShouldThrowError()
    {
        //Arrange
        var featureManager = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Description = "Test",
            Enabled = true
        };

        //Act
        await _thothFeatureManager.AddAsync(featureManager);
        var feature = async () => await _thothFeatureManager.GetEnvironmentValueAsync<int>(featureManager.Name);

        //Assert
        await feature
            .Should()
            .ThrowAsync<ThothException>()
            .WithMessage(string.Format(Messages.ERROR_WRONG_FEATURE_TYPE, featureManager.Name, "EnvironmentVariable"));
    }

    [Fact]
    public async Task GetEnvironmentValueAsync_WhenDisabled_ShouldThrowError()
    {
        //Arrange
        var featureManager = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.EnvironmentVariable,
            Description = "Test",
            Enabled = false,
            Value = "100"
        };

        //Act
        await _thothFeatureManager.AddAsync(featureManager);
        var feature = async () => await _thothFeatureManager.GetEnvironmentValueAsync<int>(featureManager.Name);

        //Assert
        await feature
            .Should()
            .ThrowAsync<ThothException>()
            .WithMessage(Messages.ERROR_CAN_NOT_GET_DISABLED_FEATURE);
    }
}