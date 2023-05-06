using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Thoth.Core;
using Thoth.Core.Models;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;
using Thoth.Sample.Contexts;
using Thoth.SQLServer;
using Thoth.Tests.Base;

namespace Thoth.Tests.SQLServerProvider;

public class ThothApiControllerTests : IntegrationTestBase<Program>
{
    private static readonly Mock<ILogger<ThothFeatureManager>> LoggerThothManager = new();

    public ThothApiControllerTests() : base(arguments: new Dictionary<string, string>
    {
        { "provider", "SQLServerProvider" }
    }, serviceDelegate: services =>
    {
        services.AddThoth(options =>
        {
            options.UseEntityFramework<SqlContext>();
            options.ShouldReturnFalseWhenNotExists = false;
        });
        services.AddScoped<ILogger<ThothFeatureManager>>(_ => LoggerThothManager.Object);
    })
    {
    }

    [Theory]
    [MemberData(nameof(CreateValidDataGenerator))]
    public async Task Create_WhenValid_ShouldSuccess(FeatureManager featureManager)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureManager), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [MemberData(nameof(CreateAndUpdateInvalidDataGenerator))]
    public async Task Create_WhenInvalid_ShouldReturnError(FeatureManager featureManager, string message)
    {
        //Arrange
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureManager), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain(message);
    }

    [Fact]
    public async Task Create_WhenInvalidAndThrow_ShouldReturnError()
    {
        //Arrange
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };
        var postContent = new StringContent(
            JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        //Act

        var response = await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, featureFlag.Name));
        LoggerThothManager.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(Messages.ERROR_WHILE_ADDIND_FEATURE_FLAG)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldSuccess()
    {
        //Act
        var response = await HttpClient.GetAsync("/thoth-api/FeatureFlag");
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<FeatureManager>>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetByName_ShouldSuccess()
    {
        //Arrange
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };
        var postContent =
            new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        //Act
        var response = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await response.Content.ReadFromJsonAsync<FeatureManager>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNull();
        content?.Name.Should().Be(featureFlag.Name);
        content?.Type.Should().Be(featureFlag.Type);
        content?.Enabled.Should().Be(featureFlag.Enabled);
    }

    [Fact]
    public async Task GetByName_WithoutCache_ShouldSuccess()
    {
        //Arrange
        var cacheManager = Services.GetRequiredService<CacheManager>();
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };
        var postContent =
            new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);
        cacheManager.Remove(featureFlag.Name);

        //Act
        var response = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await response.Content.ReadFromJsonAsync<FeatureManager>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNull();
        content?.Name.Should().Be(featureFlag.Name);
        content?.Type.Should().Be(featureFlag.Type);
        content?.Enabled.Should().Be(featureFlag.Enabled);
    }

    [Fact]
    public async Task Update_WhenValid_ShouldSuccess()
    {
        //Arrange
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };

        var postContent =
            new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        featureFlag.Enabled = false;
        featureFlag.Description = "test";
        var updateContent =
            new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        await HttpClient.PutAsync("/thoth-api/FeatureFlag", updateContent);
        var response = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await response.Content.ReadFromJsonAsync<FeatureManager>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNull();
        content?.Name.Should().Be(featureFlag.Name);
        content?.Type.Should().Be(featureFlag.Type);
        content?.Enabled.Should().Be(featureFlag.Enabled);
        content?.Description.Should().Be(featureFlag.Description);
        content?.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(CreateAndUpdateInvalidDataGenerator))]
    public async Task Update_WhenInvalid_ShouldReturnError(FeatureManager featureManager, string message)
    {
        //Arrange
        featureManager.Enabled = false;
        var updateContent =
            new StringContent(JsonConvert.SerializeObject(featureManager), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PutAsync("/thoth-api/FeatureFlag", updateContent);
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain(message);
    }

    [Fact]
    public async Task Update_WhenInvalidAndThrow_ShouldReturnError()
    {
        //Arrange
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString()
        };

        var updateContent =
            new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        var response = await HttpClient.PutAsync("/thoth-api/FeatureFlag", updateContent);
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureFlag.Name));
        LoggerThothManager.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(string.Format(Messages.ERROR_WHILE_UPDATING_FEATURE_FLAG,
                        featureFlag.Name))),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenValid_ShouldSuccess()
    {
        //Arrange
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };
        var postContent =
            new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        //Act
        var response = await HttpClient.DeleteAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var res = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await res.Content.ReadAsStringAsync();
        var error = string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureFlag.Name);

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Be(error);
    }

    [Fact]
    public async Task Delete_WhenInvalid_ShouldReturnError()
    {
        //Arrange
        var featureFlag = new FeatureManager
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };

        //Act
        var response = await HttpClient.DeleteAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureFlag.Name));
        LoggerThothManager.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(string.Format(Messages.ERROR_WHILE_DELETING_FEATURE_FLAG,
                        featureFlag.Name))),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
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
                Description = "Test",
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

    public static IEnumerable<object[]> CreateAndUpdateInvalidDataGenerator()
    {
        yield return new object[]
        {
            new FeatureManager
            {
                Type = FeatureTypes.FeatureFlag,
                SubType = FeatureFlagsTypes.Boolean
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureManager.Name))
        };
        yield return new object[]
        {
            new FeatureManager
            {
                Type = FeatureTypes.EnvironmentVariable,
                SubType = FeatureFlagsTypes.Boolean
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureManager.Name))
        };
        yield return new object[]
        {
            new FeatureManager
            {
                Type = FeatureTypes.EnvironmentVariable
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureManager.Value))
        };
        yield return new object[]
        {
            new FeatureManager
            {
                Type = FeatureTypes.FeatureFlag
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureManager.SubType))
        };
        yield return new object[]
        {
            new FeatureManager
            {
                Type = FeatureTypes.FeatureFlag,
                SubType = FeatureFlagsTypes.Boolean,
                Value = "100"
            },
            Messages.ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_VALUE
        };
        yield return new object[]
        {
            new FeatureManager
            {
                Name = Guid.NewGuid().ToString(),
                Type = FeatureTypes.FeatureFlag,
                SubType = FeatureFlagsTypes.PercentageFilter,
                Enabled = true
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureManager.Value))
        };
    }
}