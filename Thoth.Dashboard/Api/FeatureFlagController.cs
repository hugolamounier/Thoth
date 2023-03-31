using Microsoft.AspNetCore.Mvc;
using Thoth.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Thoth.Core.Models;

namespace Thoth.Dashboard.Api;

public class FeatureFlagController
{
    private readonly IFeatureFlagManagement _featureFlagManagement;

    public FeatureFlagController(IFeatureFlagManagement featureFlagManagement)
    {
        _featureFlagManagement = featureFlagManagement;
    }

    /// <summary>
    /// Get all feature flags
    /// </summary>
    /// <returns></returns>
    public async Task<IResult> GetAll()
    {
        var featureFlags = await _featureFlagManagement.GetAllAsync();

        return Results.Ok(featureFlags);
    }

    /// <summary>
    /// Get feature flag by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<IResult> GetByName(string name)
    {
        var featureFlag = await _featureFlagManagement.GetAsync(name);

        return Results.Ok(featureFlag);
    }

    /// <summary>
    /// Add a new feature flag
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <returns></returns>
    public async Task<IResult> Create(FeatureFlag featureFlag)
    {
        if (await _featureFlagManagement.AddAsync(featureFlag))
            return Results.StatusCode(201);

        return Results.BadRequest();
    }

    /// <summary>
    /// Update the feature flag
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <returns></returns>
    public async Task<IResult> Update(FeatureFlag featureFlag)
    {
        if(await _featureFlagManagement.UpdateAsync(featureFlag))
            return Results.Ok();

        return Results.BadRequest();
    }

    public async Task<IResult> Delete(string name)
    {
        if (await _featureFlagManagement.DeleteAsync(name))
            return Results.Ok();

        return Results.BadRequest();
    }
}