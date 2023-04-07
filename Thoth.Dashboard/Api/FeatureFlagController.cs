using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Dashboard.Api;

public class FeatureFlagController
{
    private readonly IThothFeatureManager _thothFeatureManager;
    private readonly ThothDashboardOptions _dashboardOptions;
    private readonly ClaimsPrincipal? _user;
    private readonly ILogger<FeatureFlagController> _logger;

    public FeatureFlagController(
        IThothFeatureManager thothFeatureManager,
        ILogger<FeatureFlagController> logger,
        HttpContext httpContext,
        ThothDashboardOptions dashboardOptions)
    {
        _thothFeatureManager = thothFeatureManager;
        _user = httpContext.User;
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
    /// <param name="featureFlag"></param>
    /// <returns></returns>
    public async Task<IResult> Create(FeatureFlag featureFlag)
    {
        if (await featureFlag.IsValidAsync(out var messages) is false)
            return Results.BadRequest(string.Join(Environment.NewLine, messages));

        if (!await _thothFeatureManager.AddAsync(featureFlag))
            return Results.BadRequest();

        _logger.LogInformation("{Message}. {ClaimInfo}",
            string.Format(Messages.INFO_ADDED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(),
                featureFlag.FilterValue), AddUserInfoToLog());

        return Results.StatusCode(201);
    }

    /// <summary>
    ///     Update the feature flag
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <returns></returns>
    public async Task<IResult> Update(FeatureFlag featureFlag)
    {
        if (await featureFlag.IsValidAsync(out var messages) is false)
            return Results.BadRequest(string.Join(Environment.NewLine, messages));

        if (!await _thothFeatureManager.UpdateAsync(featureFlag))
            return Results.BadRequest();

        _logger.LogInformation("{Message}. {ClaimInfo}",
            string.Format(Messages.INFO_UPDATED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(), featureFlag.FilterValue),
            AddUserInfoToLog());

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
            string.Format(Messages.INFO_DELETED_FEATURE_FLAG, name), AddUserInfoToLog());

        return Results.Ok();
    }

    private string AddUserInfoToLog()
    {
        if (!_dashboardOptions.ClaimsToRegisterOnLog.Any())
            return string.Empty;

        var info = new StringBuilder();

        foreach (var claimName in _dashboardOptions.ClaimsToRegisterOnLog)
        {
            var claim = _user?.Claims.FirstOrDefault(x => x.Type == claimName);
            if(claim is not null)
                info.Append($"'{claimName}': '{claim.Value}'; ");
        }

        return string.IsNullOrWhiteSpace(info.ToString()) ? 
            string.Empty : 
            string.Format(Messages.INFO_ACTION_MADE_BY_USER_WITH_CLAIMS, info);
    }
}