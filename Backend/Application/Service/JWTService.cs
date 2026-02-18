using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Microsoft.IdentityModel.Tokens;


namespace SportsTimerBackend.External.APIControllers;

/// <summary>
/// Service for handling Json Web Token token generation and management
/// </summary
public class JWTService : IJWTService
{
    private readonly IConfiguration _configuration;
    /// <summary>
    /// Initializes a new instance of the JWTService class
    /// </summary>
    /// <param name="configuration">Application configuration for accessing JWT settings</param>
    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a JWT token for User authentication
    /// </summary>
    /// <param name="user">User object containing user data for claims</param>
    /// <returns>JWT token string</returns>
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return GenerateToken(claims);
    }


    /// <summary>
    /// Retrieves the token validity period in minutes from configuration
    /// </summary>
    /// <returns>Number of minutes the token remains valid</returns>
    public int GetTokenValidityMinutes()
    {
        var jwtConfig = _configuration.GetSection("JwtConfig");
        return Convert.ToInt32(jwtConfig["TokenValidityMins"]);
    }

    /// <summary>
    /// Generates a JWT token with the provided claims
    /// </summary>
    /// <param name="claims">Array of claims to include in the token, either from Admin or Trainer</param>
    /// <returns>JWT token string</returns>
    private string GenerateToken(Claim[] claims)
    {
        var jwtConfig = _configuration.GetSection("JwtConfig");
        var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!); //This is defined in appsetting
        var tokenValidityMinutes = Convert.ToInt32(jwtConfig["TokenValidityMins"]);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenValidityMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}