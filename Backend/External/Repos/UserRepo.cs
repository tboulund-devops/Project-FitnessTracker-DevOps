using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Npgsql;

namespace Backend.External.Repos;

public class UserRepo : IUserRepo
{
    private readonly IConnectionService _connectionService;

    public UserRepo(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        throw new NotImplementedException("This method is not implemented yet.");
    }

    public async Task<User?> GetUserInfoByIdAsync(int userId)
    {
        await using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT fldUserID, fldName, fldEmail, fldTimeOfRegistration, fldTotalWorkoutTime FROM tblUser WHERE fldUserID = @userId",
            connection);
        cmd.Parameters.AddWithValue("@userId", userId);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2))
        {
            TimeOfRegistration = reader.GetDateTime(3),
            TotalWorkoutTimeMinutes = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
        };
    }

    public async Task<int> GetWorkoutCountAsync(int userId)
    {
        await using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM tblUserWorkout WHERE fldUserID = @userId",
            connection);
        cmd.Parameters.AddWithValue("@userId", userId);

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<DateTime>> GetWorkoutDatesAsync(int userId)
    {
        await using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT DISTINCT w.fldDateOfWorkout 
              FROM tblUserWorkout uw
              JOIN tblWorkout w ON uw.fldWorkoutID = w.fldWorkoutID
              WHERE uw.fldUserID = @userId
              ORDER BY w.fldDateOfWorkout DESC",
            connection);
        cmd.Parameters.AddWithValue("@userId", userId);
        await using var reader = await cmd.ExecuteReaderAsync();

        var dates = new List<DateTime>();
        while (await reader.ReadAsync())
            dates.Add(reader.GetDateTime(0));

        return dates;
    }

    public async Task<string?> GetMostUsedExerciseAsync(int userId)
    {
        await using var connection = _connectionService.GetConnection();
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT e.fldName
              FROM tblUserWorkout uw
              JOIN tblWorkoutSet ws ON uw.fldWorkoutID = ws.fldWorkoutID
              JOIN tblSet s ON ws.fldSetID = s.fldSetID
              JOIN tblExercise e ON s.fldExerciseID = e.fldExerciseID
              WHERE uw.fldUserID = @userId
              GROUP BY e.fldName
              ORDER BY COUNT(*) DESC
              LIMIT 1",
            connection);
        cmd.Parameters.AddWithValue("@userId", userId);

        return await cmd.ExecuteScalarAsync() as string;
    }
}