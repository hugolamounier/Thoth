using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;
using Thoth.Core.Models;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;
using Thoth.Dashboard.Api;
using Thoth.Tests.Base;
using Thoth.Tests.Helpers;

namespace Thoth.Tests.SQLServerProvider;

public class ThothJwtAuthorizationFilterTests: IntegrationTestBase<Program>
{
    public ThothJwtAuthorizationFilterTests() : base(arguments: new Dictionary<string, string>
    {
        {"auth", "UseThothJwtAuthorization"},
        {"provider", "SQLServerProvider"}
    })
    {
        var token = JwtGenerator.GenerateToken(new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new (ClaimTypes.Email, "thotest@thotest.thoth")
        });

        HttpClient.GetAsync($"/thoth?accessToken={token}").GetAwaiter().GetResult();
    }

    [Theory]
    [MemberData(nameof(CreateValidDataGenerator))]
    public async Task Create_ShouldBeAuthorized(FeatureManager featureManager)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureManager), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync($"/thoth-api/FeatureFlag", postContent);

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Dashboard_ShouldBeAuthorized()
    {
        //Act
        var response = await HttpClient.GetAsync($"/thoth");
        var responseShouldUseCookies = await HttpClient.GetAsync($"/thoth");

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
        yield return new object[] { JwtGenerator.GenerateToken(new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new (ClaimTypes.Email, "thotest@thotest.thoth")
        }, 1, "hhh", "tttt", new SigningCredentials
        (
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
            SecurityAlgorithms.HmacSha256Signature
        ))};

        yield return new object[] {""};
    }
}