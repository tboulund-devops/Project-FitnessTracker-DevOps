namespace Backend.Domain;

public class Workout
{
    public int? WorkoutID { get; set; }
    public DateTime DateOfWorkout { get; set; }
    public string Name { get; set; }
    
    public List<Set>? Sets { get; set; }
}