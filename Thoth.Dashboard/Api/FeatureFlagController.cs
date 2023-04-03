using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Dashboard.Api;

public class FeatureFlagController
{
    private readonly IThothFeatureManager _thothFeatureManager;

    public FeatureFlagController(IThothFeatureManager thothFeatureManager)
    {
        _thothFeatureManager = thothFeatureManager;
    }

    /// <summary>
    ///     Get all feature flags
    /// </summary>
    /// <returns></returns>
    public async Task<IResult> GetAll()
    {
        var featureFlags = await _thothFeatureManager.GetAllAsync();

        return Results.Ok(featureFlags);
    }

    /// <summary>
    ///     Get feature flag by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<IResult> GetByName(string name)
    {
        var featureFlag = await _thothFeatureManager.GetAsync(name);

        return Results.Ok(featureFlag);
    }

    /// <summary>
    ///     Add a new feature flag
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <returns></returns>
    public async Task<IResult> Create(FeatureFlag featureFlag)
    {
        if (await _thothFeatureManager.AddAsync(featureFlag))
            return Results.StatusCode(201);

        return Results.BadRequest();
    }

    /// <summary>
    ///     Update the feature flag
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <returns></returns>
    public async Task<IResult> Update(FeatureFlag featureFlag)
    {
        if (await _thothFeatureManager.UpdateAsync(featureFlag))
            return Results.Ok();

        return Results.BadRequest();
    }

    /// <summary>
    ///     Delete the feature flag
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<IResult> Delete(string name)
    {
        if (await _thothFeatureManager.DeleteAsync(name))
            return Results.Ok();

        return Results.BadRequest();
    }
}