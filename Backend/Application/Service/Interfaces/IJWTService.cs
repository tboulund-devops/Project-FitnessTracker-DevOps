using System.Security.Claims;
using Backend.Domain;

namespace Backend.Application.Service.Interfaces;
/// <summary>
/// Json Web Token Interface
/// </summary>
public interface IJWTService
{
    /// <summary>
    /// Generates a JWT token for a trainer
    /// </summary>
    /// <param name="trainer">The trainer to generate a token for</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(User trainer);

    /// <summary>
    /// Gets the token validity period in minutes
    /// </summary>
    /// <returns>Token validity in minutes</returns>
    int GetTokenValidityMinutes();

}