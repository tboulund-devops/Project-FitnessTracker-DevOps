using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.Repos.Interface;
using Microsoft.AspNetCore.Components.Sections;

namespace Backend.Application.Service;

public class WorkoutService : IWorkoutService
{
    
    private readonly IWorkoutRepo _repo;

    public WorkoutService(IWorkoutRepo repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    public async Task<int> CreateWorkout(Workout request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || userId <= 0)
        {
            throw new ArgumentException("Name is required");
        }
        
        return await _repo.CreateWorkout(request, userId);
    }

    public async Task<int> AddSetToWorkout(Set newSet, int WorkoutID)
    {

        if (newSet == null || WorkoutID <= 0)
        {
            throw new ArgumentException("Set verification failed");
        }
        
        return await _repo.AddSetToWorkout(newSet, WorkoutID);;
    }

    public async Task<Workout> GetWorkout(int workoutId)
    {
        if (workoutId <= 0)
        {
            throw new ArgumentException("Workout ID is required");
        }
        
        return await _repo.getWorkout(workoutId);
    }
    
}