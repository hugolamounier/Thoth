using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Thoth.Core;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;
using Thoth.Tests.Base;

namespace Thoth.Tests.SQLServerProvider;

public class SampleControllerTests : IntegrationTestBase<Program>
{
    private static readonly Mock<ILogger<ThothFeatureManager>> Logger = new();
    private readonly IThothFeatureManager _thothFeatureManager;

    public SampleControllerTests() : base(arguments: new Dictionary<string, string>
    {
        { "provider", "SQLServerProvider" }
    }, serviceDelegate: services => { services.AddScoped<ILogger<ThothFeatureManager>>(_ => Logger.Object); })
    {
        _thothFeatureManager = ServiceScope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
    }

    [Fact]
    public async Task GetSample_WhenBooleanType_ShouldSuccess()
    {
        //Arrange
        var newFeatureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };

        await _thothFeatureManager.AddAsync(newFeatureFlag);

        //Act
        var response = await HttpClient.GetAsync($"/Sample?featureFlagName={newFeatureFlag.Name}");

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Feature Enabled");
    }

    [Fact]
    public async Task GetSample_WhenPercentageFilterType_ShouldReturnEnabled_Success()
    {
        //Arrange
        var newFeatureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.PercentageFilter,
            Enabled = true,
            Value = "50"
        };

        await _thothFeatureManager.AddAsync(newFeatureFlag);

        //Act
        var validationTasks = Enumerable.Range(0, 1000).Select(async _ =>
        {
            var response = await HttpClient.GetAsync($"/Sample?featureFlagName={newFeatureFlag.Name}");
            response.IsSuccessStatusCode.Should().BeTrue();
            return await response.Content.ReadAsStringAsync();
        }).ToList();

        var validationResult = string.Empty;

        while (validationTasks.Any())
        {
            var finishedTask = await Task.WhenAny(validationTasks);
            if (finishedTask.Result == "Feature Enabled")
            {
                validationResult = finishedTask.Result;
                validationTasks.RemoveAll(_ => true);
            }

            validationTasks.Remove(finishedTask);
        }

        validationResult.Should().Be("Feature Enabled");
    }

    [Theory]
    [InlineData(false, "Feature Disabled")]
    [InlineData(true, "Feature Enabled")]
    public async Task GetSample_WhenPercentageFilterType_ShouldReturnDisabled_Success(bool isActive,
        string validationMessage)
    {
        //Arrange
        var newFeatureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.PercentageFilter,
            Enabled = isActive,
            Value = "100"
        };

        await _thothFeatureManager.AddAsync(newFeatureFlag);

        //Act
        var response = await HttpClient.GetAsync($"/Sample?featureFlagName={newFeatureFlag.Name}");
        response.IsSuccessStatusCode.Should().BeTrue();
        var validationResult = await response.Content.ReadAsStringAsync();

        //Assert
        validationResult.Should().Be(validationMessage);
    }

    [Fact]
    public async Task GetSample_WhenPercentageFilterTypeInvalid_ShouldReturnDisabled_Success()
    {
        //Arrange
        var newFeatureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.PercentageFilter,
            Enabled = true,
            Value = "0"
        };

        await _thothFeatureManager.AddAsync(newFeatureFlag);

        //Act
        var response = await HttpClient.GetAsync($"/Sample?featureFlagName={newFeatureFlag.Name}");
        response.IsSuccessStatusCode.Should().BeTrue();
        var validationResult = await response.Content.ReadAsStringAsync();

        //Assert
        validationResult.Should().Be("Feature Disabled");
        Logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains(
                        "When using 'PercentageFilter' flag type, 'Value' must be defined and be greater than zero (0) for the feature:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }
}