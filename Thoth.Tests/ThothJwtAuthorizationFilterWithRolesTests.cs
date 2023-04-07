using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Thoth.Core.Models;
using Thoth.Tests.Base;
using Thoth.Tests.Helpers;

namespace Thoth.Tests;

public class ThothJwtAuthorizationFilterWithRolesTests: IntegrationTestBase<Program>
{
    private readonly string _token;
    
    public ThothJwtAuthorizationFilterWithRolesTests() : base(arguments: new Dictionary<string, string>
    {
        {"auth", "UseThothJwtAuthorizationWithRoles"}
    })
    {
        _token = JwtGenerator.GenerateToken(new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new (ClaimTypes.Email, "thotest@thotest.thoth"),
            new (ClaimTypes.Role, "Admin")
        });
    }
    
    [Theory]
    [MemberData(nameof(CreateValidDataGenerator))]
    public async Task Create_ShouldBeAuthorized(FeatureFlag featureFlag)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync($"/thoth-api/FeatureFlag?accessToken={_token}", postContent);

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_ShouldBeAuthorized()
    {
        //Act
        var response = await HttpClient.GetAsync($"/thoth/?accessToken={_token}");
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

    public static IEnumerable<object[]> InvalidTokenDataGenerator()
    {
        yield return new object[] { JwtGenerator.GenerateToken(new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new (ClaimTypes.Email, "thotest@thotest.thoth"),
            new (ClaimTypes.Role, "User")
        }, 1, "hhh", "tttt", new SigningCredentials
        (
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
            SecurityAlgorithms.HmacSha256Signature
        ))};

        yield return new object[] {""};
    }
}