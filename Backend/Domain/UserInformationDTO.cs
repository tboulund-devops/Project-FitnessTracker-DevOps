namespace Backend.Domain;

public class UserInformationDTO
{
    public string Name { get; set; } = string.Empty;
    public DateTime TimeOfRegistry { get; set; }
    public int TotalAmountOfWorkouts { get; set; }
    public string TotalAmountOfTimeTrained { get; set; } = "0h 0m";
    public int CurrentStreakDays { get; set; }
    public int CurrentStreakWeeks { get; set; }
    public int BestStreakDays { get; set; }
    public int BestStreakWeeks { get; set; }
    public string FavoriteExercise { get; set; } = string.Empty;
}