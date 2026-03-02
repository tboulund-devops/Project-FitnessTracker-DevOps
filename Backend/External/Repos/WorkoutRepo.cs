using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.Repos.Interface;
using Npgsql;

namespace Backend.External.Repos;

public class WorkoutRepo : IWorkoutRepo
{
    
    private readonly IConnectionService _connectionService;

    public WorkoutRepo(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<int> CreateWorkout(Workout workout, int userId)
    {
        
        // Handle null
        if (workout == null || string.IsNullOrEmpty(workout.Name))
        {
            throw new Exception("Workout cannot be null or empty");
        }
        
        
        using var connection = _connectionService.GetConnection();
        connection.Open();
        int workoutId;
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
            INSERT INTO tblWorkout (fldDateOfWorkout, fldName)
            VALUES (@dateOfWorkout, @name)
            RETURNING fldWorkoutID;";
        
            insertCmd.Parameters.AddWithValue("@dateOfWorkout", workout.DateOfWorkout);
            insertCmd.Parameters.AddWithValue("@name", workout.Name);
            
            var result = await insertCmd.ExecuteScalarAsync();
            workoutId = Convert.ToInt32(result);
            if (workoutId == null)
            {
                throw new Exception("Failed to create workout");
            }
        }
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
            INSERT INTO tblUserWorkout (fldWorkoutID, fldUserID)
            VALUES (@workoutId, @userId)
            RETURNING fldUserWorkoutID;";
        
            insertCmd.Parameters.AddWithValue("@workoutId", workoutId);
            insertCmd.Parameters.AddWithValue("@userId", userId);
            
            var result = await insertCmd.ExecuteScalarAsync();
            
            if (result == null)
            {
                throw new Exception("Failed to add workout to bridge table");
            }
            return Convert.ToInt32(result);
        }
        
    }
}