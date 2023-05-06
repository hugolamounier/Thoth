using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.Dashboard.Api;

public class FeatureManagerController
{
    private readonly ThothDashboardOptions _dashboardOptions;
    private readonly IThothFeatureManager _thothFeatureManager;

    public FeatureManagerController(
        IThothFeatureManager thothFeatureManager,
        ThothDashboardOptions dashboardOptions)
    {
        _thothFeatureManager = thothFeatureManager;
        _dashboardOptions = dashboardOptions;
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
    /// <param name="featureManager"></param>
    /// <returns></returns>
    public async Task<IResult> Create(FeatureManager featureManager)
    {
        if (await featureManager.IsValidAsync(out var messages) is false)
            return Results.BadRequest(string.Join(Environment.NewLine, messages));

        if (_dashboardOptions.ThothManagerAudit is not null)
            featureManager.Extras = _dashboardOptions.ThothManagerAudit.AddAuditExtras();

        if (!await _thothFeatureManager.AddAsync(featureManager))
            return Results.BadRequest();

        return Results.StatusCode(201);
    }

    /// <summary>
    ///     Update the feature flag
    /// </summary>
    /// <param name="featureManager"></param>
    /// <returns></returns>
    public async Task<IResult> Update(FeatureManager featureManager)
    {
        if (await featureManager.IsValidAsync(out var messages) is false)
            return Results.BadRequest(string.Join(Environment.NewLine, messages));

        if (_dashboardOptions.ThothManagerAudit is not null)
            featureManager.Extras = _dashboardOptions.ThothManagerAudit.AddAuditExtras();

        if (!await _thothFeatureManager.UpdateAsync(featureManager))
            return Results.BadRequest();

        return Results.Ok();
    }

    /// <summary>
    ///     Delete the feature flag
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<IResult> Delete(string name)
    {
        if (!await _thothFeatureManager.DeleteAsync(name))
            return Results.BadRequest();

        return Results.Ok();
    }
}