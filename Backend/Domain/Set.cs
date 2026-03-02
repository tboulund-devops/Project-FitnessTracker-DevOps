namespace Backend.Domain;

public class Set
{
    public int? SetID { get; set; }
    public int ExerciseID { get; set; }
    public int Weight { get; set; }
    public int Reps { get; set; }
    public int RestBetweenSetInSec { get; set; }
}