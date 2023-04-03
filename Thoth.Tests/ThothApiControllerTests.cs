using System.Net;
using System.Net.Http.Json;
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
    
    [Theory]
    [MemberData(nameof(CreateInvalidDataGenerator))]
    public async Task Create_WhenInvalid_ShouldReturnError(FeatureFlag featureFlag, string message)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain(message);
    }

    [Fact]
    public async Task GetAll_ShouldSuccess()
    {
        //Act
        var response = await HttpClient.GetAsync("/thoth-api/FeatureFlag");
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<FeatureFlag>>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetByName_ShouldSuccess()
    {
        //Arrange
        var featureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true,
        };
        var postContent = new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        //Act
        var response = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await response.Content.ReadFromJsonAsync<FeatureFlag>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNull();
        content?.Name.Should().Be(featureFlag.Name);
        content?.Type.Should().Be(featureFlag.Type);
        content?.Value.Should().Be(featureFlag.Value);
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
    
    public static IEnumerable<object[]> CreateInvalidDataGenerator()
    {
        yield return new object[] { new FeatureFlag
        {
            Type = FeatureFlagsTypes.Boolean
        }, string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureFlag.Name)) };
        yield return new object[] { new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.PercentageFilter,
            Value = true
        }, string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureFlag.FilterValue)) };
    }
}