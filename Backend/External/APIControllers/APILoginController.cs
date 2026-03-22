using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Backend.External.APIControllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class APILoginController : ControllerBase
{

    private readonly ILoginService _loginService;

    public APILoginController(ILoginService loginService)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
    }
    
    
    [HttpGet("Login_CheckCredentials")]
    public ActionResult<int> CheckCredentials(LoginRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized("Credtials Can not be empty");
        }

        var returnUserId = _loginService.CheckCredentials(request);

        if (returnUserId <= 0)
        {
            return Unauthorized("Invalid username or password");
        }
        
        return Ok(returnUserId); 
    }
    [HttpPost("Register")]
    public IActionResult RegisterLoginCredentials(RegisterUserRequest? request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required");
        }

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Username, password, name and email are required");
        }

        if (request.TotalWorkoutTime > 0)
        {
            return BadRequest("Total workout time must be less that 1");
        }

        
        bool isAdded = _loginService.RegisterLoginCredentials(request);

        if (!isAdded)
        {
            return Conflict("Registration failed. Username may already exist");
        }
        
        return Ok("Credentials registered. User profile fields received");
    }
    
}
