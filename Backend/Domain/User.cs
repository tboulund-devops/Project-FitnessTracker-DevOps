namespace Backend.Domain;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime TimeOfRegistration { get; set; }
    public int TotalWorkoutTimeMinutes { get; set; }

    public User(int id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
        TimeOfRegistration = DateTime.UtcNow;
        TotalWorkoutTimeMinutes = 0;
    }
}