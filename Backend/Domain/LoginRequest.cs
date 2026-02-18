namespace Backend.Domain;

/// <summary>
/// Request model for login containing credentials
/// </summary>
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}