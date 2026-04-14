﻿using Backend.Application.Service;
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
    public async Task<ActionResult<int>> CreateWorkout(Workout request, int UserId)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name) || UserId <= 0)
        {
            return BadRequest("Workout must need a creating date and name, and have a positive user id");
        }

        int returnWorkoutID = await _workoutService.CreateWorkout(request, UserId);

        if (returnWorkoutID <= 0)
        {
            return NotFound("Unable to create workout");
        }
        
        return Ok(returnWorkoutID); 
        
    }

    [HttpPost("AddSetToWorkout")]
    public async Task<IActionResult> AddSetToWorkout(Set newSet, int WorkoutID)
    {
        if (newSet.Reps <= 0 || newSet.ExerciseID <= 0 || newSet.Weight <= 0)
        { 
            return BadRequest("Set verification failure");
        }

        if (WorkoutID <= 0)
        {
            return BadRequest("Workout id must be a positive number");
        }
        int isValid = await _workoutService.AddSetToWorkout(newSet, WorkoutID);
        
        if (isValid <= 0)
        {
            return NotFound("Unable to Add Set to workout: " + WorkoutID);
        }

        return Ok("Set successfully added to  workout: " + WorkoutID);
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
    
    [HttpGet("GetWorkoutsByUserID/{userId}")]
    public async Task<ActionResult<List<Workout>>> GetWorkoutsByUserID(int userId)
    {
        if (userId <= 0)
            return BadRequest("User id must be a positive number");

        var workouts = await _workoutService.GetWorkoutsByUserID(userId);
    
        if (workouts == null || workouts.Count == 0)
            return NotFound($"No workouts found for user with ID {userId}");

        return Ok(workouts);
     }
    
    
    
}
