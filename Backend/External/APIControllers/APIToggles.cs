using FeaturehubHelper;
using Microsoft.AspNetCore.Mvc;

namespace Backend.External.APIControllers;

[ApiController]
[Route("api/toggle/[controller]")]
public class APIToggles: ControllerBase
{
    private readonly FeatureStateProvider _provider;

    public APIToggles(FeatureStateProvider provider)
    {
        _provider = provider;
    }

    [HttpGet("feature-toggles/{feature}")]
    public ActionResult<bool> GetFeatureToggle(string feature)
    {
        if (string.IsNullOrWhiteSpace(feature))
        {
            return BadRequest("Feature name must be provided");
        }

        bool isEnabled = _provider.IsEnabled(feature);

        return Ok(isEnabled);
    }
}