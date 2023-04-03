using Microsoft.AspNetCore.Mvc;
using Thoth.Core.Interfaces;

namespace Thoth.Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    private readonly IThothFeatureManager _thothFeatureManager;

    public SampleController(IThothFeatureManager thothFeatureManager)
    {
        _thothFeatureManager = thothFeatureManager;
    }

    [HttpGet(Name = "TestFeatureFlagDelivery")]
    public async Task<IActionResult> Get([FromQuery] string featureFlagName)
    {
        if (await _thothFeatureManager.IsEnabledAsync(featureFlagName))
            return Ok("Feature Enabled");
        
        return Ok("Feature Disabled");
    }
}