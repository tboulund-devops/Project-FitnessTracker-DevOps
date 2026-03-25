using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.Repos;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace UnitTests.Backend_UnitTest.UserTests;

public class UserRepoTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private IConnectionService _connectionService;
    private UserRepo _userRepo;

    public UserRepoTests()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgreSqlContainer.GetConnectionString()
            })
            .Build();

        _connectionService = new ConnectionService(configuration);

        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();

        var createTablesCmd = connection.CreateCommand();
        createTablesCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS tblUserCredentials (
                fldCredentialsID SERIAL PRIMARY KEY,
                fldUsername VARCHAR(100) NOT NULL,
                fldPassword VARCHAR(100) NOT NULL,
                CONSTRAINT uq_credentials_username UNIQUE (fldUsername)
            );

            CREATE TABLE IF NOT EXISTS tblUser (
                fldUserID SERIAL PRIMARY KEY,
                fldCredentialsID INT NOT NULL,
                fldName VARCHAR(100) NOT NULL,
                fldEmail VARCHAR(100) NOT NULL,
                fldTotalWorkoutTime INT,
                fldTimeOfRegistration TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (fldCredentialsID) REFERENCES tblUserCredentials(fldCredentialsID),
                CONSTRAINT uq_user_credentials UNIQUE (fldCredentialsID)
            );

            CREATE TABLE IF NOT EXISTS tblWorkout (
                fldWorkoutID SERIAL PRIMARY KEY,
                fldDateOfWorkout DATE NOT NULL,
                fldName VARCHAR(100) NOT NULL,
                CONSTRAINT uq_workout_date_name UNIQUE (fldDateOfWorkout, fldName)
            );

            CREATE TABLE IF NOT EXISTS tblUserWorkout (
                fldUserWorkoutID SERIAL PRIMARY KEY,
                fldUserID INT NOT NULL,
                fldWorkoutID INT NOT NULL,
                FOREIGN KEY (fldUserID) REFERENCES tblUser(fldUserID),
                FOREIGN KEY (fldWorkoutID) REFERENCES tblWorkout(fldWorkoutID),
                CONSTRAINT uq_user_workout UNIQUE (fldUserID, fldWorkoutID)
            );

            CREATE TABLE IF NOT EXISTS tblExercise (
                fldExerciseID SERIAL PRIMARY KEY,
                fldName VARCHAR(100) NOT NULL,
                fldDescription VARCHAR(100) NOT NULL,
                CONSTRAINT uq_exercise_name UNIQUE (fldName)
            );

            CREATE TABLE IF NOT EXISTS tblSet (
                fldSetID SERIAL PRIMARY KEY,
                fldExerciseID INT NOT NULL,
                fldWeight INT NOT NULL,
                fldReps INT NOT NULL,
                fldRestBetweenSet INT NOT NULL,
                FOREIGN KEY (fldExerciseID) REFERENCES tblExercise(fldExerciseID),
                CONSTRAINT uq_set_definition UNIQUE (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
            );

            CREATE TABLE IF NOT EXISTS tblWorkoutSet (
                fldWorkoutSetID SERIAL PRIMARY KEY,
                fldSetID INT NOT NULL,
                fldWorkoutID INT NOT NULL,
                FOREIGN KEY (fldSetID) REFERENCES tblSet(fldSetID),
                FOREIGN KEY (fldWorkoutID) REFERENCES tblWorkout(fldWorkoutID),
                CONSTRAINT uq_workout_set UNIQUE (fldSetID, fldWorkoutID)
            );
        ";
        await createTablesCmd.ExecuteNonQueryAsync();

        // Seed user with workout time
        var seedCmd = connection.CreateCommand();
        seedCmd.CommandText = @"
            INSERT INTO tblUserCredentials (fldUsername, fldPassword)
            VALUES ('testuser', 'testpass');

            INSERT INTO tblUser (fldCredentialsID, fldName, fldEmail, fldTotalWorkoutTime)
            VALUES (1, 'Test User', 'test@example.com', 150);

            -- Second user with NULL workout time
            INSERT INTO tblUserCredentials (fldUsername, fldPassword)
            VALUES ('nulluser', 'testpass');

            INSERT INTO tblUser (fldCredentialsID, fldName, fldEmail, fldTotalWorkoutTime)
            VALUES (2, 'Null Time User', 'null@example.com', NULL);

            -- Exercises
            INSERT INTO tblExercise (fldName, fldDescription)
            VALUES ('Bench Press', 'Chest exercise'),
                   ('Squat', 'Leg exercise');

            -- Workouts on consecutive days
            INSERT INTO tblWorkout (fldWorkoutID, fldDateOfWorkout, fldName)
            VALUES (1, '2026-03-09', 'Workout A'),
                   (2, '2026-03-10', 'Workout B');

            -- Link workouts to user 1
            INSERT INTO tblUserWorkout (fldUserID, fldWorkoutID) VALUES (1, 1);
            INSERT INTO tblUserWorkout (fldUserID, fldWorkoutID) VALUES (1, 2);

            -- Sets: 3 Bench Press sets, 1 Squat set (Bench Press is favorite)
            INSERT INTO tblSet (fldSetID, fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
            VALUES (1, 1, 80, 10, 60),
                   (2, 1, 85, 8, 60),
                   (3, 1, 90, 6, 90),
                   (4, 2, 100, 5, 120);

            -- Link sets to workouts
            INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID) VALUES (1, 1);
            INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID) VALUES (2, 1);
            INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID) VALUES (3, 2);
            INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID) VALUES (4, 2);
        ";
        await seedCmd.ExecuteNonQueryAsync();

        _userRepo = new UserRepo(_connectionService);
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    // ===================== Constructor Tests =====================

    [DockerFact]
    public void Constructor_WithValidConnectionService_InitializesRepo()
    {
        var repo = new UserRepo(_connectionService);
        Assert.NotNull(repo);
    }

    // ===================== GetByUsernameAsync Tests =====================

    [DockerFact]
    public async Task GetByUsernameAsync_AnyUsername_ThrowsNotImplementedException()
    {
        var ex = await Assert.ThrowsAsync<NotImplementedException>(
            () => _userRepo.GetByUsernameAsync("testuser"));
        Assert.Contains("not implemented", ex.Message);
    }

    [DockerFact]
    public async Task GetByUsernameAsync_EmptyUsername_ThrowsNotImplementedException()
    {
        var ex = await Assert.ThrowsAsync<NotImplementedException>(
            () => _userRepo.GetByUsernameAsync(""));
        Assert.Contains("not implemented", ex.Message);
    }

    [DockerFact]
    public async Task GetByUsernameAsync_NullUsername_ThrowsNotImplementedException()
    {
        var ex = await Assert.ThrowsAsync<NotImplementedException>(
            () => _userRepo.GetByUsernameAsync(null!));
        Assert.Contains("not implemented", ex.Message);
    }

    // ===================== GetUserInfoByIdAsync Tests =====================

    [DockerFact]
    public async Task GetUserInfoByIdAsync_ExistingUser_ReturnsUser()
    {
        var result = await _userRepo.GetUserInfoByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test User", result.Username);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal(150, result.TotalWorkoutTimeMinutes);
        Assert.True(result.TimeOfRegistration > DateTime.MinValue);
    }

    [DockerFact]
    public async Task GetUserInfoByIdAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _userRepo.GetUserInfoByIdAsync(999);

        Assert.Null(result);
    }

    [DockerFact]
    public async Task GetUserInfoByIdAsync_UserWithNullWorkoutTime_ReturnsZero()
    {
        var result = await _userRepo.GetUserInfoByIdAsync(2);

        Assert.NotNull(result);
        Assert.Equal("Null Time User", result.Username);
        Assert.Equal(0, result.TotalWorkoutTimeMinutes);
    }

    // ===================== GetWorkoutCountAsync Tests =====================

    [DockerFact]
    public async Task GetWorkoutCountAsync_UserWithWorkouts_ReturnsCorrectCount()
    {
        var result = await _userRepo.GetWorkoutCountAsync(1);

        Assert.Equal(2, result);
    }

    [DockerFact]
    public async Task GetWorkoutCountAsync_UserWithNoWorkouts_ReturnsZero()
    {
        var result = await _userRepo.GetWorkoutCountAsync(2);

        Assert.Equal(0, result);
    }

    [DockerFact]
    public async Task GetWorkoutCountAsync_NonExistingUser_ReturnsZero()
    {
        var result = await _userRepo.GetWorkoutCountAsync(999);

        Assert.Equal(0, result);
    }

    // ===================== GetWorkoutDatesAsync Tests =====================

    [DockerFact]
    public async Task GetWorkoutDatesAsync_UserWithWorkouts_ReturnsDatesDescending()
    {
        var result = await _userRepo.GetWorkoutDatesAsync(1);

        Assert.Equal(2, result.Count);
        Assert.True(result[0] > result[1]); // descending order
        Assert.Equal(new DateTime(2026, 3, 10), result[0]);
        Assert.Equal(new DateTime(2026, 3, 9), result[1]);
    }

    [DockerFact]
    public async Task GetWorkoutDatesAsync_UserWithNoWorkouts_ReturnsEmptyList()
    {
        var result = await _userRepo.GetWorkoutDatesAsync(2);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [DockerFact]
    public async Task GetWorkoutDatesAsync_NonExistingUser_ReturnsEmptyList()
    {
        var result = await _userRepo.GetWorkoutDatesAsync(999);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // ===================== GetMostUsedExerciseAsync Tests =====================

    [DockerFact]
    public async Task GetMostUsedExerciseAsync_UserWithExercises_ReturnsMostUsedExercise()
    {
        var result = await _userRepo.GetMostUsedExerciseAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Bench Press", result); // 3 sets vs 1 Squat set
    }

    [DockerFact]
    public async Task GetMostUsedExerciseAsync_UserWithNoSets_ReturnsNull()
    {
        var result = await _userRepo.GetMostUsedExerciseAsync(2);

        Assert.Null(result);
    }

    [DockerFact]
    public async Task GetMostUsedExerciseAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _userRepo.GetMostUsedExerciseAsync(999);

        Assert.Null(result);
    }

    // ===================== AddUserInformation Tests =====================

    [DockerFact]
    public async Task AddUserInformation_ValidData_ReturnsTrue()
    {
        await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await connection.OpenAsync();

        var seedCredentials = connection.CreateCommand();
        seedCredentials.CommandText = "INSERT INTO tblUserCredentials (fldUsername, fldPassword) VALUES ('newuser', 'password123')";
        await seedCredentials.ExecuteNonQueryAsync();

        var result = _userRepo.addUserInformation(3, "New User", "newuser@example.com", 30);

        Assert.True(result);
    }

    [DockerFact]
    public void AddUserInformation_InvalidNameOrEmail_ReturnsFalse()
    {
        Assert.False(_userRepo.addUserInformation(1, null, "valid@example.com", 10));
        Assert.False(_userRepo.addUserInformation(1, "Valid Name", null, 10));
        Assert.False(_userRepo.addUserInformation(1, " ", "valid@example.com", 10));
    }

    [DockerFact]
    public void AddUserInformation_NegativeWorkoutTime_ReturnsFalse()
    {
        var result = _userRepo.addUserInformation(1, "Valid Name", "valid@example.com", -1);

        Assert.False(result);
    }

    [DockerFact]
    public void AddUserInformation_InvalidCredentialsId_ReturnsFalse()
    {
        var result = _userRepo.addUserInformation(0, "invaliduser", "invaliduser@example.com", 10);

        Assert.False(result);
    }
}
