using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.Gateway;
using Backend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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

    
    [HttpPost("Login_CheckCredentials")]
    public IActionResult CheckCredentials(LoginRequest request)
    {
        if (request == null ||string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized("Credtials Can not be empty");
        }

        var isValid = _loginService.CheckCredentials(request);

        if (!isValid)
        {
            return Unauthorized("Invalid username or password");
        }
        
        return Ok("Valid Credentials"); 
        
    }
}
