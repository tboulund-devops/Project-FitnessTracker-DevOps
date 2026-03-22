namespace Backend.Domain;

/// <summary>
/// Request model for registering login credentials and initial user profile data.
/// </summary>
public class RegisterUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalWorkoutTime { get; set; } = 0;
}

