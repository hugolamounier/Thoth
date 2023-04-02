using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Thoth.Core.Models;
using Thoth.Tests.Base;

namespace Thoth.Tests;

public class ThothApiControllerTests : IntegrationTestBase<Program>
{
    [Theory]
    [MemberData(nameof(CreateValidDataGenerator))]
    public async Task Create_WhenValid_ShouldSuccess(FeatureFlag featureFlag)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }
    
    public static IEnumerable<object[]> CreateValidDataGenerator()
    {
        yield return new object[] { new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
        } };
        yield return new object[] { new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.PercentageFilter,
            FilterValue = "50",
            Value = true
        } };
    }
}