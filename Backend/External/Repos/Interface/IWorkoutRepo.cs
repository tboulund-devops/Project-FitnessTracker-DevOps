using Backend.Domain;

namespace Backend.External.Repos.Interface;

public interface IWorkoutRepo
{
    public Task<int> CreateWorkout(Workout request, int userId);
    public Task<int> AddSetToWorkout(Set newSet, int WorkoutID);
}