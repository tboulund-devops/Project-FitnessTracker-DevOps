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
        if (workout == null || string.IsNullOrEmpty(workout.Name))
            throw new Exception("Workout cannot be null or empty");

        using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();
        
        //transaction combines the 2 inserts to a single "unit" / commit, if not, the first might succede
        //and the 2nd fail, and you are left with "floating" data in tblWorkout.
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            int workoutId;
            using (var insertWorkoutCmd = connection.CreateCommand())
            {
                insertWorkoutCmd.Transaction = transaction;
                insertWorkoutCmd.CommandText = @"
                INSERT INTO tblWorkout (fldDateOfWorkout, fldName)
                VALUES (@dateOfWorkout, @name)
                RETURNING fldWorkoutID;";

                insertWorkoutCmd.Parameters.AddWithValue("@dateOfWorkout", workout.DateOfWorkout);
                insertWorkoutCmd.Parameters.AddWithValue("@name", workout.Name);

                var result = await insertWorkoutCmd.ExecuteScalarAsync();
                if (result == null)
                    throw new Exception("Failed to create workout");
                workoutId = Convert.ToInt32(result);
            }

            using (var insertBridgeCmd = connection.CreateCommand())
            {
                insertBridgeCmd.Transaction = transaction;
                insertBridgeCmd.CommandText = @"
                INSERT INTO tblUserWorkout (fldWorkoutID, fldUserID)
                VALUES (@workoutId, @userId)
                RETURNING fldUserWorkoutID;";

                insertBridgeCmd.Parameters.AddWithValue("@workoutId", workoutId);
                insertBridgeCmd.Parameters.AddWithValue("@userId", userId);

                var result = await insertBridgeCmd.ExecuteScalarAsync();
                if (result == null)
                    throw new Exception("Failed to add workout to bridge table");

                await transaction.CommitAsync();
                return Convert.ToInt32(result);
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddSetToWorkout(Set setRequest, int workoutId)
{
    if (setRequest == null)
    {
        throw new ArgumentNullException(nameof(setRequest));
    }

    using var connection = _connectionService.GetConnection();
    await connection.OpenAsync();
    //transaction combines the 2 inserts to a single "unit" / commit, if not, the first might succede
    //and the 2nd fail, and you are left with "floating" data in tblSet.
    using var transaction = await connection.BeginTransactionAsync();

    try
    {
        // 1. Insert the set into tblSet
        int setId;
        using (var insertSetCmd = connection.CreateCommand())
        {
            insertSetCmd.Transaction = transaction;
            insertSetCmd.CommandText = @"
                INSERT INTO tblSet (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
                VALUES (@exerciseId, @weight, @reps, @rest)
                RETURNING fldSetID;";

            insertSetCmd.Parameters.AddWithValue("@exerciseId", setRequest.ExerciseID);
            insertSetCmd.Parameters.AddWithValue("@weight", setRequest.Weight);
            insertSetCmd.Parameters.AddWithValue("@reps", setRequest.Reps);
            insertSetCmd.Parameters.AddWithValue("@rest", setRequest.RestBetweenSetInSec);

            var result = await insertSetCmd.ExecuteScalarAsync();
            if (result == null)
                throw new Exception("Failed to create set");

            setId = Convert.ToInt32(result);
        }

        // 2. Link the set to the workout in tblWorkoutSet
        using (var insertBridgeCmd = connection.CreateCommand())
        {
            insertBridgeCmd.Transaction = transaction;
            insertBridgeCmd.CommandText = @"
                INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID)
                VALUES (@setId, @workoutId)
                RETURNING fldWorkoutSetID;";

            insertBridgeCmd.Parameters.AddWithValue("@setId", setId);
            insertBridgeCmd.Parameters.AddWithValue("@workoutId", workoutId);

            var result = await insertBridgeCmd.ExecuteScalarAsync();
            if (result == null)
                throw new Exception("Failed to link set to workout");

            var workoutSetId = Convert.ToInt32(result);

            await transaction.CommitAsync();
            return workoutSetId; // Return the bridge table ID
        }
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
}