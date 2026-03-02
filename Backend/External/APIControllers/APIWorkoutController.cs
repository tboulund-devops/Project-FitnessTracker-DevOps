using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Backend.External.APIControllers;

[ApiController]
[Route("api/workout/[controller]")]
public class APIWorkoutController : ControllerBase
{

    private readonly WorkoutService _workoutService;

    public APIWorkoutController(WorkoutService workoutService)
    {
        _workoutService = workoutService ?? throw new ArgumentNullException(nameof(workoutService));
    }
    
    [HttpPost("CreateWorkout")]
    public IActionResult CheckCredentials(Workout request, int UserId)
    {
        if (request == null ||request.DateOfWorkout == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return Unauthorized("Workout must need a creating date and name");
        }

        var isValid = _workoutService.CreateWorkout(request, UserId);

        if (!isValid)
        {
            return Unauthorized("Unable to create workout");
        }
        
        return Ok("Workout created successfully"); 
        
    }
    
    
    
    
}
