using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Tests.Base;

namespace Thoth.Tests;

public class SampleControllerIntegrationTest : IntegrationTestBase<Program>
{

    [Fact]
    public async Task GetSample_WhenBooleanType_ShouldSuccess()
    {
        //Arrange
        var thothFeatureManager = ServiceScope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
        var newFeatureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
        };
        
        await thothFeatureManager.AddAsync(newFeatureFlag);

        //Act
        var response = await HttpClient.GetAsync($"/Sample?featureFlagName={newFeatureFlag.Name}");

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Feature Enabled");
    }
}