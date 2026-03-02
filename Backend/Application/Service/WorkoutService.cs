using Backend.Domain;
using Backend.External.Repos.Interface;

namespace Backend.Application.Service;

public class WorkoutService
{
    
    private readonly IWorkoutRepo _repo;
    public bool CreateWorkout(Workout request, int userId)
    {
        int returnIDOfBridgetable = Convert.ToInt32(_repo.CreateWorkout(request, userId));

        if (returnIDOfBridgetable <= 0)
        {
            return false;
        }
        return true;
    }
    
    
    
}