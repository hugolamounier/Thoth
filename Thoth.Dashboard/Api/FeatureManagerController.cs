using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Core.Models.Entities;

namespace Thoth.Dashboard.Api;

public class FeatureManagerController
{
    private readonly IThothFeatureManager _thothFeatureManager;
    private readonly ThothDashboardOptions _dashboardOptions;
    private readonly ILogger<FeatureManagerController> _logger;

    public FeatureManagerController(
        IThothFeatureManager thothFeatureManager,
        ILogger<FeatureManagerController> logger,
        ThothDashboardOptions dashboardOptions)
    {
        _thothFeatureManager = thothFeatureManager;
        _logger = logger;
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

        if (!await _thothFeatureManager.AddAsync(featureManager))
            return Results.BadRequest();

        _logger.LogInformation("{Message}. {ClaimInfo}",
            string.Format(Messages.INFO_ADDED_FEATURE_FLAG, featureManager.Name, featureManager.Enabled.ToString(),
                featureManager.Value), _dashboardOptions.AuditExtras?.AddAuditExtras() ?? string.Empty );

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

        if (!await _thothFeatureManager.UpdateAsync(featureManager))
            return Results.BadRequest();

        _logger.LogInformation("{Message}. {ClaimInfo}",
            string.Format(Messages.INFO_UPDATED_FEATURE_FLAG,
                featureManager.Name,
                featureManager.Enabled.ToString(),
                featureManager.Value),
            _dashboardOptions.AuditExtras?.AddAuditExtras() ?? string.Empty);

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

        _logger.LogInformation("{Message}. {ClaimInfo}",
            string.Format(Messages.INFO_DELETED_FEATURE_FLAG, name),
            _dashboardOptions.AuditExtras?.AddAuditExtras() ?? string.Empty);

        return Results.Ok();
    }
}