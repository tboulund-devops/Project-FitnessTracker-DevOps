namespace SportsTimerBackend.Domain;

/// <summary>
/// Request model for login containing credentials
/// </summary>
public class TrainerLoginRequest
{
    public string Username { get; set; } 
    public string Password { get; set; }
}