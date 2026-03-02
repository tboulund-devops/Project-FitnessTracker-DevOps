using Backend.Domain;
using Backend.External.Repos.Interface;
using Microsoft.AspNetCore.Components.Sections;

namespace Backend.Application.Service;

public class WorkoutService
{
    
    private readonly IWorkoutRepo _repo;

    public int CreateWorkout(Workout request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || userId <= 0)
        {
            throw new ArgumentException("Name is required");
        }

        int returnIdOfBridgetableUserWorkout = Convert.ToInt32(_repo.CreateWorkout(request, userId));

        return returnIdOfBridgetableUserWorkout;
    }

    public int AddSetToWorkout(Set newSet, int WorkoutID)
    {

        if (newSet == null || WorkoutID <= 0)
        {
            throw new ArgumentException("Set verification failed");
        }
        
        int returnIdOfBridgeTableWorkoutSet = Convert.ToInt32(_repo.AddSetToWorkout(newSet, WorkoutID));
        return returnIdOfBridgeTableWorkoutSet;
    }
    
    
    
}