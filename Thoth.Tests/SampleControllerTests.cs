using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Tests.Base;

namespace Thoth.Tests;

public class SampleControllerTests : IntegrationTestBase<Program>
{
    private readonly IThothFeatureManager _thothFeatureManager;

    public SampleControllerTests() 
    {
        _thothFeatureManager = ServiceScope.ServiceProvider.GetRequiredService<IThothFeatureManager>();;
    }

    [Fact]
    public async Task GetSample_WhenBooleanType_ShouldSuccess()
    {
        //Arrange
        var newFeatureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
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
        var newFeatureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.PercentageFilter,
            Value = true,
            FilterValue = "50" 
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
    public async Task GetSample_WhenPercentageFilterType_ShouldReturnDisabled_Success(bool isActive, string validationMessage)
    {
        //Arrange
        var newFeatureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.PercentageFilter,
            Value = isActive,
            FilterValue = "100"
        };
        
        await _thothFeatureManager.AddAsync(newFeatureFlag);

        //Act
        var response = await HttpClient.GetAsync($"/Sample?featureFlagName={newFeatureFlag.Name}");
        response.IsSuccessStatusCode.Should().BeTrue();
        var validationResult = await response.Content.ReadAsStringAsync();

        //Assert
        validationResult.Should().Be(validationMessage);
    }
}