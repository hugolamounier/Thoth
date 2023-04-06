using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Thoth.Core;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Dashboard.Api;
using Thoth.Tests.Base;

namespace Thoth.Tests;

public class ThothApiControllerTests : IntegrationTestBase<Program>
{
    private static readonly Mock<ILogger<FeatureFlagController>> Logger = new();
    private static readonly Mock<ILogger<ThothFeatureManager>> LoggerThothManager = new();

    public ThothApiControllerTests() : base(services =>
    {
        services.AddThoth(options =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            options.ConnectionString = configuration.GetConnectionString("SqlContext");
            options.ShouldReturnFalseWhenNotExists = false;
        });
        services.AddScoped<ILogger<FeatureFlagController>>(_ => Logger.Object);
        services.AddScoped<ILogger<ThothFeatureManager>>(_ => LoggerThothManager.Object);
    }) { }

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
        Logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(Messages.INFO_ADDED_FEATURE_FLAG, featureFlag.Name,
                    featureFlag.Value.ToString(),
                    featureFlag.FilterValue))),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }

    [Theory]
    [MemberData(nameof(CreateAndUpdateInvalidDataGenerator))]
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
    public async Task Create_WhenInvalidAndThrow_ShouldReturnError()
    {
        //Arrange
        var featureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
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
            Value = true
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

    [Fact]
    public async Task Update_WhenValid_ShouldSuccess()
    {
        //Arrange
        var featureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
        };
        var postContent = new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/thoth-api/FeatureFlag", postContent);

        featureFlag.Value = false;
        var updateContent = new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

        //Act
        await HttpClient.PutAsync("/thoth-api/FeatureFlag", updateContent);
        var response = await HttpClient.GetAsync($"/thoth-api/FeatureFlag/{featureFlag.Name}");
        var content = await response.Content.ReadFromJsonAsync<FeatureFlag>();

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().NotBeNull();
        content?.Name.Should().Be(featureFlag.Name);
        content?.Type.Should().Be(featureFlag.Type);
        content?.Value.Should().Be(featureFlag.Value);
        content?.UpdatedAt.Should().NotBeNull();
        Logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(Messages.INFO_UPDATED_FEATURE_FLAG, featureFlag.Name,
                    featureFlag.Value.ToString(),
                    featureFlag.FilterValue))),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }

    [Theory]
    [MemberData(nameof(CreateAndUpdateInvalidDataGenerator))]
    public async Task Update_WhenInvalid_ShouldReturnError(FeatureFlag featureFlag, string message)
    {
        //Arrange
        featureFlag.Value = false;
        var updateContent = new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

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
        var featureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString()
        };

        var updateContent = new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(Messages.ERROR_WHILE_UPDATING_FEATURE_FLAG, featureFlag.Name))),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenValid_ShouldSuccess()
    {
        //Arrange
        var featureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
        };
        var postContent = new StringContent(JsonConvert.SerializeObject(featureFlag), Encoding.UTF8, "application/json");
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
        var featureFlag = new FeatureFlag
        {
            Name = Guid.NewGuid().ToString(),
            Type = FeatureFlagsTypes.Boolean,
            Value = true
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(Messages.ERROR_WHILE_DELETING_FEATURE_FLAG, featureFlag.Name))),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
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

    public static IEnumerable<object[]> CreateAndUpdateInvalidDataGenerator()
    {
        yield return new object[]
        {
            new FeatureFlag
            {
                Type = FeatureFlagsTypes.Boolean
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureFlag.Name))
        };
        yield return new object[]
        {
            new FeatureFlag
            {
                Name = Guid.NewGuid().ToString(),
                Type = FeatureFlagsTypes.PercentageFilter,
                Value = true
            },
            string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FeatureFlag.FilterValue))
        };
    }
}