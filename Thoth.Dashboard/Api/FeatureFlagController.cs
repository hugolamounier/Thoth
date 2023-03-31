using Microsoft.AspNetCore.Mvc;
using Thoth.Core.Interfaces;

namespace Thoth.Dashboard.Api;

[Route("thoth/[controller]")]
public class FeatureFlagController: ControllerBase
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
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var featureFlags = await _featureFlagManagement.GetAllAsync();

        return Ok(featureFlags);
    }
}