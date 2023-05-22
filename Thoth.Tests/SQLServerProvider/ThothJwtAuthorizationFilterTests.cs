using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;
using Thoth.Tests.Base;
using Thoth.Tests.Helpers;

namespace Thoth.Tests.SQLServerProvider;

public class ThothJwtAuthorizationFilterTests : IntegrationTestBase<Program>
{
    public ThothJwtAuthorizationFilterTests() : base(arguments: new Dictionary<string, string>
    {
        { "auth", "UseThothJwtAuthorization" },
        { "provider", "SQLServerProvider" }
    })
    {
        var token = JwtGenerator.GenerateToken(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "thotest@thotest.thoth"),
            new(ClaimTypes.Role, "Admin")
        });

        HttpClient.GetAsync($"/thoth?accessToken={token}").GetAwaiter().GetResult();
    }

    [Theory]
    [MemberData(nameof(CreateValidDataGenerator))]
    public async Task CreateAndUpdate_ShouldBeAuthorizedAndAudit(FeatureManager featureManager)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureManager), Encoding.UTF8, "application/json");
        featureManager.Enabled = !featureManager.Enabled;
        var putContent = new StringContent(
            JsonConvert.SerializeObject(featureManager), Encoding.UTF8, "application/json");

        //Act
        var responseCreate = await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);
        var createdFeature = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureManager.Name}");
        var responseUpdate = await HttpClient.PutAsync("/thoth-api/FeatureFlag", putContent);
        var updatedFeature = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureManager.Name}");
        var createdContent = await createdFeature.Content.ReadFromJsonAsync<FeatureManager>();
        var updatedContent = await updatedFeature.Content.ReadFromJsonAsync<FeatureManager>();


        //Assert
        responseCreate.IsSuccessStatusCode.Should().BeTrue();
        responseCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        responseUpdate.IsSuccessStatusCode.Should().BeTrue();
        responseUpdate.StatusCode.Should().Be(HttpStatusCode.OK);
        createdFeature.IsSuccessStatusCode.Should().BeTrue();
        updatedFeature.IsSuccessStatusCode.Should().BeTrue();
        createdContent.Should().NotBeNull();
        createdContent?.Extras.Should().NotBeEmpty().And.Contain("thotest@thotest.thoth");
        updatedContent.Should().NotBeNull();
        updatedContent?.Enabled.Should().Be(featureManager.Enabled);
        updatedContent?.Extras.Should().NotBeEmpty().And.Contain("thotest@thotest.thoth");
    }

    [Fact]
    public async Task Dashboard_ShouldBeAuthorized()
    {
        //Act
        var response = await HttpClient.GetAsync("/thoth");
        var responseShouldUseCookies = await HttpClient.GetAsync("/thoth");

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        responseShouldUseCookies.IsSuccessStatusCode.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidTokenDataGenerator))]
    public async Task Dashboard_ShouldBeForbidden(string token)
    {
        //Act
        var response = await HttpClient.GetAsync($"/thoth?accessToken={token}");


        //Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static IEnumerable<object[]> CreateValidDataGenerator()
    {
        yield return new object[]
        {
            new FeatureManager
            {
                Name = Guid.NewGuid().ToString(),
                Type = FeatureTypes.FeatureFlag,
                SubType = FeatureFlagsTypes.Boolean,
                Enabled = true
            }
        };
        yield return new object[]
        {
            new FeatureManager
            {
                Name = Guid.NewGuid().ToString(),
                Type = FeatureTypes.FeatureFlag,
                SubType = FeatureFlagsTypes.PercentageFilter,
                Value = "50",
                Enabled = true
            }
        };
    }

    public static IEnumerable<object[]> InvalidTokenDataGenerator()
    {
        yield return new object[]
        {
            JwtGenerator.GenerateToken(new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Email, "thotest@thotest.thoth")
            }, 1, "hhh", "tttt", new SigningCredentials
            (
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
                SecurityAlgorithms.HmacSha256Signature
            ))
        };

        yield return new object[] { "" };
    }
}