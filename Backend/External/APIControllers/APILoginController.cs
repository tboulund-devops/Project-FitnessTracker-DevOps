using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTimerBackend.Domain;
using Npgsql;

namespace Backend.External.APIControllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class APILoginController : ControllerBase, IAPILoginController
{
    private readonly IConfiguration _configuration;

    public APILoginController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    
    [HttpPost("Login_CheckCredentials")]
    public bool CheckCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Username and password are required");
        }
        
        return Unauthorized("Invalid credentials");
    }
    
    [HttpGet]
    public IActionResult GetUsers()
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, username FROM tbllogincred", conn);
        using var reader = cmd.ExecuteReader();

        var users = new List<object>();

        while (reader.Read())
        {
            users.Add(new
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1)
            });
        }

        return Ok(users);
    }
}
