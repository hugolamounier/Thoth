using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;
using Thoth.Tests.Base;

namespace Thoth.Tests.MongoDbProvider;

public class ThothMongoDbProviderTests : IntegrationTestBase<Program>
{
    private readonly IThothFeatureManager _thothFeatureManager;
    private readonly IMongoCollection<FeatureManager> _mongoCollection;
    private readonly string _featureFlagName = "TestDeleteTllExpiresAt";

    public ThothMongoDbProviderTests() : base(arguments: new Dictionary<string, string>
    {
        { "provider", "MongoDbProvider" }
    })
    {
        _thothFeatureManager = ServiceScope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
        var mongoClient = ServiceScope.ServiceProvider.GetRequiredService<IMongoClient>();
        _mongoCollection = mongoClient.GetDatabase("thoth")
            .GetCollection<FeatureManager>("thoth");
    }

    [Fact]
    public async Task Update_ShouldAddHistoryAndHaveDeleteTtlSet_Success()
    {
        //Arrange
        var featureToAdd = new FeatureManager
        {
            Name = _featureFlagName,
            Type = FeatureTypes.FeatureFlag,
            SubType = FeatureFlagsTypes.Boolean,
            Enabled = true
        };

        await _thothFeatureManager.AddAsync(featureToAdd);
        featureToAdd.Enabled = false;

        //Act
        await _thothFeatureManager.UpdateAsync(featureToAdd);
        var featureFlag = await _thothFeatureManager.GetAsync(featureToAdd.Name);

        //Assert
        featureFlag?.Should().NotBeNull();
        featureFlag?.Enabled.Should().BeFalse();
        featureFlag?.Histories.Should().NotBeEmpty();
        featureFlag?.Histories.First().Enabled.Should().Be(!featureToAdd.Enabled);
    }

    [Fact]
    public async Task Delete_ShouldUpdateExpiresAtAndShouldExpireAfterTtl_Success()
    {
        //Act
        await _thothFeatureManager.DeleteAsync(_featureFlagName);
        var feature = await _mongoCollection
            .Find(c => c.Name == _featureFlagName)
            .FirstOrDefaultAsync();

        //Assert
        feature.Should().NotBeNull();
        feature?.ExpiresAt.Should().NotBeNull().And.Be(feature.DeletedAt + TimeSpan.FromSeconds(5));

        await Task.Delay(10000);
        feature = await _mongoCollection
            .Find(c => c.Name == _featureFlagName)
            .FirstOrDefaultAsync();

        feature.Should().BeNull();
    }
}