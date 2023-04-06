using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Thoth.Core.Models;
using Thoth.Tests.Base;
using Thoth.Tests.Helpers;

namespace Thoth.Tests;

public class ThothJwtAuthorizationFilterTests: IntegrationTestBase<Program>
{
    public ThothJwtAuthorizationFilterTests() : base(arguments: new Dictionary<string, string>
    {
        {"auth", "UseThothJwtAuthorization"}
    }) { }

    [Theory]
    [MemberData(nameof(CreateValidDataGenerator))]
    public async Task Create_ShouldReturnForbidden(FeatureFlag featureFlag)
    {
        //Arrange
        var token = JwtGenerator.GenerateToken(new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new (ClaimTypes.Email, "thotest@thotest.thoth")
        });

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync($"/thoth-api/FeatureFlag?accessToken={token}", postContent);

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    public static IEnumerable<object[]> CreateValidDataGenerator()
    {
        yield return new object[]
        {
            new FeatureFlag
            {
                Name = Guid.NewGuid().ToString(),
                Type = FeatureFlagsTypes.Boolean,
                Value = true
            }
        };
        yield return new object[]
        {
            new FeatureFlag
            {
                Name = Guid.NewGuid().ToString(),
                Type = FeatureFlagsTypes.PercentageFilter,
                FilterValue = "50",
                Value = true
            }
        };
    }
}