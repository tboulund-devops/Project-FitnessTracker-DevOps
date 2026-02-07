using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SportsTimerBackend.Domain;
using SportsTimerBackend.External.APIControllers.ServiceInterfaces;

namespace Backend.External.APIControllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class APIAuthController : ControllerBase
{
    private readonly UserService _trainerService;
    private readonly IJWTService _jwtService;

    /// <summary>
    /// Initializes a new instance of the APIAuthController class
    /// </summary>
    /// <param name="userService">Service for User-related operations</param>
    /// <param name="adminService">Service for admin-related operations</param>
    /// <param name="jwtService">Service for JWT token generation</param>
    public APIAuthController(UserService userService, IJWTService jwtService)
    {
        _trainerService = userService;
        _adminService = adminService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Authenticates a trainer and returns JWT token upon successful login
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>
    /// 200 OK with TrainerDTO containing trainer data and access token,
    /// 401 Unauthorized if credentials are invalid
    /// </returns>
    [AllowAnonymous]
    [HttpPost("trainer-login")]
    public async Task<ActionResult<TrainerDTO>> Login([FromBody] LoginRequest request)
    {
        Trainer foundTrainer = await _trainerService.AuthenticateAsync(request.TrainerEmail.ToLower(), request.TrainerPassword);

        if (foundTrainer == null)
        {
            return Unauthorized("Invalid credentials");
        }
        // Generate JWT token for Trainer
        var token = _jwtService.GenerateToken(foundTrainer);
        var expiresIn = _jwtService.GetTokenValidityMinutes() * 60;

        return Ok(new TrainerDTO
        {
            Trainer = foundTrainer,
            AccessToken = token,
            ExpiresIn = expiresIn
        });
    }
}
