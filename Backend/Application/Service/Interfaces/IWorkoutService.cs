using Backend.Domain;

namespace Backend.Application.Service.Interfaces;

public interface IWorkoutService
{
    Task<int> CreateWorkout(Workout request, int userId);
    Task<int> AddSetToWorkout(Set newSet, int WorkoutID);
    Task<Workout> GetWorkout(int workoutId);
}