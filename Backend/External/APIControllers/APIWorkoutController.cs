using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Messages.Authentication;

namespace Backend.External.APIControllers;

[ApiController]
[Route("api/workout/[controller]")]
public class APIWorkoutController : ControllerBase
{

    private readonly IWorkoutService _workoutService;

    public APIWorkoutController(IWorkoutService workoutService)
    {
        _workoutService = workoutService ?? throw new ArgumentNullException(nameof(workoutService));
    }
    
    [HttpPost("CreateWorkout")]
    public async Task<IActionResult> CheckCredentials(Workout request, int UserId)
    {
        if (request == null ||request.DateOfWorkout == null || string.IsNullOrWhiteSpace(request.Name) || UserId <= 0)
        {
            return BadRequest("Workout must need a creating date and name, and have a positive user id");
        }

        int isValid = await _workoutService.CreateWorkout(request, UserId);

        if (isValid <= 0)
        {
            return NotFound("Unable to create workout");
        }
        
        return Ok("Workout created successfully"); 
        
    }

    [HttpPost("AddSetToWorkout")]
    public async Task<IActionResult> AddSetToWorkout(Set setRequest, int workoutId)
    {
        if (setRequest.Reps <= 0 || setRequest.ExerciseID <= 0 || setRequest.Weight <= 0)
        { 
            return BadRequest("Set verification failure");
        }

        if (workoutId <= 0)
        {
            return BadRequest("Workout id must be a positive number");
        }
        int isValid = await _workoutService.AddSetToWorkout(setRequest, workoutId);
        
        if (isValid <= 0)
        {
            return NotFound("Unable to Add Set to workout: " + workoutId);
        }

        return Ok("Set successfully added to  workout: " + workoutId);
    }

    [HttpGet("GetWorkoutInformation/{workoutId}")]
    public async Task<ActionResult<Workout>> GetWorkoutsForUser(int workoutId)
    {
        if (workoutId <= 0)
            return BadRequest("Workout id must be a positive number");

        var workout = await _workoutService.GetWorkout(workoutId);
    
        if (workout == null)
            return NotFound($"Workout with ID {workoutId} not found");

        return Ok(workout);
    }
    
    
    
}
