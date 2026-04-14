﻿using Backend.Application.Service.Interfaces;
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

    public async Task<int> CreateWorkout(Workout request, int userId)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            throw new Exception("Workout cannot be null or empty");

        using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();
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

                insertWorkoutCmd.Parameters.AddWithValue("@dateOfWorkout", request.DateOfWorkout);
                insertWorkoutCmd.Parameters.AddWithValue("@name", request.Name);

                var result = await insertWorkoutCmd.ExecuteScalarAsync();
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

                await transaction.CommitAsync();
                return workoutId; // Return the actual workout ID, not the bridge table ID
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddSetToWorkout(Set newSet, int WorkoutID) 
    {
        if (newSet == null)
        {
            throw new ArgumentNullException(nameof(newSet));
        }

        using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();
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

                insertSetCmd.Parameters.AddWithValue("@exerciseId", newSet.ExerciseID);
                insertSetCmd.Parameters.AddWithValue("@weight", newSet.Weight);
                insertSetCmd.Parameters.AddWithValue("@reps", newSet.Reps);
                insertSetCmd.Parameters.AddWithValue("@rest", newSet.RestBetweenSetInSec);

                var result = await insertSetCmd.ExecuteScalarAsync();
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
                insertBridgeCmd.Parameters.AddWithValue("@workoutId", WorkoutID);

                var result = await insertBridgeCmd.ExecuteScalarAsync();
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

    public async Task<Workout> getWorkout(int workoutId)
    {
        using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();

        // First, retrieve the workout details
        Workout workout = null;
        using (var getWorkoutCmd = connection.CreateCommand())
        {
            getWorkoutCmd.CommandText = @"
            SELECT fldWorkoutID, fldDateOfWorkout, fldName
            FROM tblWorkout
            WHERE fldWorkoutID = @workoutId;";
        
            getWorkoutCmd.Parameters.AddWithValue("@workoutId", workoutId);

            using var reader = await getWorkoutCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                workout = new Workout
                {
                    WorkoutID = reader.GetInt32(0),
                    DateOfWorkout = reader.GetDateTime(1),
                    Name = reader.GetString(2),
                    Sets = new List<Set>()
                };
            }
        }

        // If workout not found, return null
        if (workout == null)
        {
            return null;
        }

        // retrieve all sets associated with this workout
        using (var getSetsCmd = connection.CreateCommand())
        {
            getSetsCmd.CommandText = @"
            SELECT s.fldSetID, s.fldExerciseID, s.fldWeight, s.fldReps, s.fldRestBetweenSet
            FROM tblSet s
            INNER JOIN tblWorkoutSet ws ON s.fldSetID = ws.fldSetID
            WHERE ws.fldWorkoutID = @workoutId;";
        
            getSetsCmd.Parameters.AddWithValue("@workoutId", workoutId);

            using var reader = await getSetsCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var set = new Set
                {
                    SetID = Convert.ToInt32(reader[0]),
                    ExerciseID = Convert.ToInt32(reader[1]),
                    Weight = Convert.ToInt32(reader[2]),
                    Reps = Convert.ToInt32(reader[3]),
                    RestBetweenSetInSec = Convert.ToInt32(reader[4])
                };
                workout.Sets.Add(set);
            }
        }
        return workout;
    }
    
   public async Task<List<Workout>> GetWorkoutsByUserID(int userId)
{
    var workouts = new List<Workout>();
    var workoutDict = new Dictionary<int, Workout>();

    using var connection = _connectionService.GetConnection();
    await connection.OpenAsync();

    using var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        SELECT 
            w.fldWorkoutID, 
            w.fldDateOfWorkout, 
            w.fldName,
            s.fldSetID,
            s.fldExerciseID,
            e.fldName as ExerciseName,
            s.fldWeight,
            s.fldReps,
            s.fldRestBetweenSet
        FROM tblWorkout w
        INNER JOIN tblUserWorkout uw ON w.fldWorkoutID = uw.fldWorkoutID
        LEFT JOIN tblWorkoutSet ws ON w.fldWorkoutID = ws.fldWorkoutID
        LEFT JOIN tblSet s ON ws.fldSetID = s.fldSetID
        LEFT JOIN tblExercise e ON s.fldExerciseID = e.fldExerciseID 
        WHERE uw.fldUserID = @userId
        ORDER BY w.fldDateOfWorkout DESC, w.fldWorkoutID;";
    
    cmd.Parameters.AddWithValue("@userId", userId);

    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var workoutId = reader.GetInt32(0);
        
        if (!workoutDict.TryGetValue(workoutId, out var workout))
        {
            workout = new Workout
            {
                WorkoutID = workoutId,
                DateOfWorkout = reader.GetDateTime(1),
                Name = reader.GetString(2),
                Sets = new List<Set>()
            };
            workoutDict.Add(workoutId, workout);
            workouts.Add(workout);
        }

        if (!reader.IsDBNull(3))
        {
            var set = new Set
            {
                SetID = reader.GetInt32(3),
                ExerciseID = reader.GetInt32(4),
                ExerciseName = reader.GetString(5),
                Weight = reader.GetInt32(6),
                Reps = reader.GetInt32(7),
                RestBetweenSetInSec = reader.GetInt32(8)
            };
            workout.Sets.Add(set);
        }
    }

    return workouts;
}
}
