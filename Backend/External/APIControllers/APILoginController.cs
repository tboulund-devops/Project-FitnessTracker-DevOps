using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTimerBackend.Domain;

namespace Backend.External.APIControllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class APIAuthController : ControllerBase
{
    /// <summary>
    /// Authenticates a trainer and returns JWT token upon successful login
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>
    /// 200 OK if credentials are accepted, 401 Unauthorized otherwise
    /// </returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required");
        }

        // NOTE: Authentication/JWT services are not currently wired in this project.
        // This endpoint is kept functional and can be extended once services exist.
        return Unauthorized("Invalid credentials");
    }
}
