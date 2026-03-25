using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.Repos;

namespace Backend.Tests.WorkoutTests
{
    public class WorkoutRepoTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private IConnectionService _connectionService;
        private WorkoutRepo _workoutRepo;

        public WorkoutRepoTests()
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

            // Seed a user for CreateWorkout tests (needs a valid userId with FK)
            var seedCmd = connection.CreateCommand();
            seedCmd.CommandText = @"
                INSERT INTO tblUserCredentials (fldUsername, fldPassword)
                VALUES ('testuser', 'testpass');

                INSERT INTO tblUser (fldCredentialsID, fldName, fldEmail)
                VALUES (1, 'Test User', 'test@example.com');

                INSERT INTO tblExercise (fldName, fldDescription)
                VALUES ('Bench Press', 'Chest exercise'),
                       ('Squat', 'Leg exercise');
            ";
            await seedCmd.ExecuteNonQueryAsync();

            _workoutRepo = new WorkoutRepo(_connectionService);
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        // ===================== CreateWorkout Tests =====================

        [DockerFact]
        public async Task CreateWorkout_ValidWorkout_ReturnsUserWorkoutId()
        {
            // Arrange
            var workout = new Workout
            {
                Name = "Morning Workout",
                DateOfWorkout = new DateTime(2026, 3, 6)
            };

            // Act
            var result = await _workoutRepo.CreateWorkout(workout, 1);

            // Assert
            Assert.True(result > 0);
        }

        [DockerFact]
        public async Task CreateWorkout_NullWorkout_ThrowsException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _workoutRepo.CreateWorkout(null!, 1));

            Assert.Equal("Workout cannot be null or empty", ex.Message);
        }

        [DockerFact]
        public async Task CreateWorkout_EmptyName_ThrowsException()
        {
            // Arrange
            var workout = new Workout
            {
                Name = "",
                DateOfWorkout = new DateTime(2026, 3, 6)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _workoutRepo.CreateWorkout(workout, 1));

            Assert.Equal("Workout cannot be null or empty", ex.Message);
        }

        [DockerFact]
        public async Task CreateWorkout_NullName_ThrowsException()
        {
            // Arrange
            var workout = new Workout
            {
                Name = null!,
                DateOfWorkout = new DateTime(2026, 3, 6)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _workoutRepo.CreateWorkout(workout, 1));

            Assert.Equal("Workout cannot be null or empty", ex.Message);
        }

        [DockerFact]
        public async Task CreateWorkout_InvalidUserId_ThrowsException()
        {
            // Arrange - userId 999 does not exist, FK violation triggers rollback
            var workout = new Workout
            {
                Name = "Bad User Workout",
                DateOfWorkout = new DateTime(2026, 4, 1)
            };

            // Act & Assert - the FK violation causes a Postgres exception, caught and re-thrown after rollback
            await Assert.ThrowsAsync<PostgresException>(
                () => _workoutRepo.CreateWorkout(workout, 999));
        }

        // ===================== AddSetToWorkout Tests =====================

        [DockerFact]
        public async Task AddSetToWorkout_ValidSet_ReturnsWorkoutSetId()
        {
            // Arrange - first create a workout
            var workout = new Workout
            {
                Name = "Set Test Workout",
                DateOfWorkout = new DateTime(2026, 3, 7)
            };
            await _workoutRepo.CreateWorkout(workout, 1);

            // Get the workoutId (it's 1 since it's the first workout in this test context,
            // but let's query it to be safe)
            await using var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT fldWorkoutID FROM tblWorkout WHERE fldName = 'Set Test Workout'";
            var workoutId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            var set = new Set
            {
                ExerciseID = 1, // Bench Press
                Weight = 100,
                Reps = 10,
                RestBetweenSetInSec = 60
            };

            // Act
            var result = await _workoutRepo.AddSetToWorkout(set, workoutId);

            // Assert
            Assert.True(result > 0);
        }

        [DockerFact]
        public async Task AddSetToWorkout_NullSet_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _workoutRepo.AddSetToWorkout(null!, 1));
        }

        [DockerFact]
        public async Task AddSetToWorkout_InvalidWorkoutId_ThrowsException()
        {
            // Arrange - workoutId 999 does not exist, FK violation triggers rollback
            var set = new Set
            {
                ExerciseID = 1,
                Weight = 50,
                Reps = 8,
                RestBetweenSetInSec = 90
            };

            // Act & Assert
            await Assert.ThrowsAsync<PostgresException>(
                () => _workoutRepo.AddSetToWorkout(set, 999));
        }

        [DockerFact]
        public async Task AddSetToWorkout_InvalidExerciseId_ThrowsException()
        {
            // Arrange - create a valid workout first
            var workout = new Workout
            {
                Name = "Invalid Exercise Workout",
                DateOfWorkout = new DateTime(2026, 3, 8)
            };
            await _workoutRepo.CreateWorkout(workout, 1);

            await using var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT fldWorkoutID FROM tblWorkout WHERE fldName = 'Invalid Exercise Workout'";
            var workoutId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            var set = new Set
            {
                ExerciseID = 999, // does not exist
                Weight = 50,
                Reps = 8,
                RestBetweenSetInSec = 90
            };

            // Act & Assert - FK violation on exerciseId triggers rollback
            await Assert.ThrowsAsync<PostgresException>(
                () => _workoutRepo.AddSetToWorkout(set, workoutId));
        }

        // ===================== getWorkout Tests =====================

        [DockerFact]
        public async Task GetWorkout_ExistingWorkoutWithSets_ReturnsWorkoutWithSets()
        {
            // Arrange - create workout and add sets
            var workout = new Workout
            {
                Name = "Full Workout",
                DateOfWorkout = new DateTime(2026, 3, 9)
            };
            await _workoutRepo.CreateWorkout(workout, 1);

            await using var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT fldWorkoutID FROM tblWorkout WHERE fldName = 'Full Workout'";
            var workoutId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            var set1 = new Set { ExerciseID = 1, Weight = 80, Reps = 12, RestBetweenSetInSec = 60 };
            var set2 = new Set { ExerciseID = 2, Weight = 120, Reps = 8, RestBetweenSetInSec = 90 };

            await _workoutRepo.AddSetToWorkout(set1, workoutId);
            await _workoutRepo.AddSetToWorkout(set2, workoutId);

            // Act
            var result = await _workoutRepo.getWorkout(workoutId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workoutId, result.WorkoutID);
            Assert.Equal("Full Workout", result.Name);
            Assert.Equal(new DateTime(2026, 3, 9), result.DateOfWorkout);
            Assert.NotNull(result.Sets);
            Assert.Equal(2, result.Sets.Count);

            // Verify set data
            Assert.Contains(result.Sets, s => s.ExerciseID == 1 && s.Weight == 80 && s.Reps == 12 && s.RestBetweenSetInSec == 60);
            Assert.Contains(result.Sets, s => s.ExerciseID == 2 && s.Weight == 120 && s.Reps == 8 && s.RestBetweenSetInSec == 90);
        }

        [DockerFact]
        public async Task GetWorkout_ExistingWorkoutWithoutSets_ReturnsWorkoutWithEmptySetsList()
        {
            // Arrange - create workout without adding sets
            var workout = new Workout
            {
                Name = "Empty Workout",
                DateOfWorkout = new DateTime(2026, 3, 10)
            };
            await _workoutRepo.CreateWorkout(workout, 1);

            await using var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT fldWorkoutID FROM tblWorkout WHERE fldName = 'Empty Workout'";
            var workoutId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            // Act
            var result = await _workoutRepo.getWorkout(workoutId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Empty Workout", result.Name);
            Assert.NotNull(result.Sets);
            Assert.Empty(result.Sets);
        }

        [DockerFact]
        public async Task GetWorkout_NonExistingWorkout_ReturnsNull()
        {
            // Act
            var result = await _workoutRepo.getWorkout(99999);

            // Assert
            Assert.Null(result);
        }

        // ===================== GetWorkoutsByUserID Tests =====================

        [DockerFact]
        public async Task GetWorkoutsByUserID_UserWithNoWorkouts_ReturnsEmptyList()
        {
            var result = await _workoutRepo.GetWorkoutsByUserID(2);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [DockerFact]
        public async Task GetWorkoutsByUserID_WorkoutWithoutSets_ReturnsWorkoutWithEmptySets()
        {
            var workout = new Workout
            {
                Name = "No Set Workout",
                DateOfWorkout = new DateTime(2026, 3, 11)
            };
            var workoutId = await _workoutRepo.CreateWorkout(workout, 1);

            var result = await _workoutRepo.GetWorkoutsByUserID(1);
            var mappedWorkout = Assert.Single(result.Where(w => w.WorkoutID == workoutId));

            Assert.Equal("No Set Workout", mappedWorkout.Name);
            Assert.NotNull(mappedWorkout.Sets);
            Assert.Empty(mappedWorkout.Sets);
        }

        [DockerFact]
        public async Task GetWorkoutsByUserID_WorkoutWithMultipleSets_ReturnsSingleWorkoutWithAllSets()
        {
            var workout = new Workout
            {
                Name = "Mapped Workout",
                DateOfWorkout = new DateTime(2026, 3, 12)
            };
            var workoutId = await _workoutRepo.CreateWorkout(workout, 1);

            await _workoutRepo.AddSetToWorkout(new Set
            {
                ExerciseID = 1,
                Weight = 90,
                Reps = 8,
                RestBetweenSetInSec = 60
            }, workoutId);

            await _workoutRepo.AddSetToWorkout(new Set
            {
                ExerciseID = 2,
                Weight = 120,
                Reps = 5,
                RestBetweenSetInSec = 90
            }, workoutId);

            var result = await _workoutRepo.GetWorkoutsByUserID(1);
            var mappedWorkout = Assert.Single(result.Where(w => w.WorkoutID == workoutId));

            Assert.Equal(2, mappedWorkout.Sets.Count);
            Assert.Contains(mappedWorkout.Sets, s => s.ExerciseID == 1 && s.ExerciseName == "Bench Press");
            Assert.Contains(mappedWorkout.Sets, s => s.ExerciseID == 2 && s.ExerciseName == "Squat");
        }

        [DockerFact]
        public async Task GetWorkoutsByUserID_ReturnsOnlyRequestedUsersWorkouts_InDateDescendingOrder()
        {
            var userOneOldWorkoutId = await _workoutRepo.CreateWorkout(new Workout
            {
                Name = "User1 Old",
                DateOfWorkout = new DateTime(2026, 3, 1)
            }, 1);

            var userOneNewWorkoutId = await _workoutRepo.CreateWorkout(new Workout
            {
                Name = "User1 New",
                DateOfWorkout = new DateTime(2026, 3, 20)
            }, 1);

            await using (var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString()))
            {
                await conn.OpenAsync();
                var createUser2Cmd = conn.CreateCommand();
                createUser2Cmd.CommandText = @"
                    INSERT INTO tblUserCredentials (fldUsername, fldPassword)
                    VALUES ('testuser2', 'testpass2');

                    INSERT INTO tblUser (fldCredentialsID, fldName, fldEmail)
                    VALUES (2, 'Test User 2', 'test2@example.com');";
                await createUser2Cmd.ExecuteNonQueryAsync();
            }

            await _workoutRepo.CreateWorkout(new Workout
            {
                Name = "User2 Workout",
                DateOfWorkout = new DateTime(2026, 3, 25)
            }, 2);

            var result = await _workoutRepo.GetWorkoutsByUserID(1);
            var ids = result.Select(w => w.WorkoutID).ToList();

            Assert.Contains(userOneOldWorkoutId, ids);
            Assert.Contains(userOneNewWorkoutId, ids);
            Assert.DoesNotContain(result, w => w.Name == "User2 Workout");

            var oldIndex = result.FindIndex(w => w.WorkoutID == userOneOldWorkoutId);
            var newIndex = result.FindIndex(w => w.WorkoutID == userOneNewWorkoutId);
            Assert.True(newIndex < oldIndex);
        }
    }
}
