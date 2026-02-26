using Npgsql;
using System.Data;
using Backend.Application.Service.Interfaces;

namespace Backend.Application.Service;

public class DatabaseSeedingService :IDatabaseSeedingService
{
    private readonly IConnectionService _connectionService;

    public DatabaseSeedingService(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public void Seed()
    {
        SeedTables();
        SeedTestData();
    }

    public void SeedTables()
    {
        using var connection = _connectionService.GetConnection();
        connection.Open();

        DropAllTables(connection);
        CreateUserCredentialsTable(connection);
        CreateUserTable(connection);
        CreateWorkoutTable(connection);
        CreateUserWorkoutTable(connection);
        CreateExerciseTable(connection);
        CreateSetTable(connection);
        CreateWorkoutSetTable(connection);
    }

    private void DropAllTables(NpgsqlConnection connection)
    {
        using (var dropCmd = connection.CreateCommand())
        {
            dropCmd.CommandText = @"
                DROP TABLE IF EXISTS tblWorkoutSet CASCADE;
                DROP TABLE IF EXISTS tblSet CASCADE;
                DROP TABLE IF EXISTS tblExercise CASCADE;
                DROP TABLE IF EXISTS tblUserWorkout CASCADE;
                DROP TABLE IF EXISTS tblWorkout CASCADE;
                DROP TABLE IF EXISTS tblUser CASCADE;
                DROP TABLE IF EXISTS tblUserCredentials CASCADE;";
            dropCmd.ExecuteNonQuery();
        }
    }

    private void CreateUserCredentialsTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblUserCredentials (
                    fldCredentialsID SERIAL PRIMARY KEY,
                    fldUsername VARCHAR(100) NOT NULL,
                    fldPassword VARCHAR(100) NOT NULL
                );";
            createCmd.ExecuteNonQuery();
        }
    }

    private void CreateUserTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblUser (
                    fldUserID SERIAL PRIMARY KEY,
                    fldCredentialsID INT NOT NULL,
                    fldName VARCHAR(100) NOT NULL,
                    fldEmail VARCHAR(100) NOT NULL,
                    fldTotalWorkoutTime INT,
                    fldTimeOfRegistration TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (fldCredentialsID) REFERENCES tblUserCredentials(fldCredentialsID)
                );";
            createCmd.ExecuteNonQuery();
        }
    }

    private void CreateWorkoutTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblWorkout (
                    fldWorkoutID SERIAL PRIMARY KEY,
                    fldDateOfWorkout DATE NOT NULL,
                    fldName VARCHAR(100) NOT NULL
                );";
            createCmd.ExecuteNonQuery();
        }
    }

    private void CreateUserWorkoutTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblUserWorkout (
                    fldUserWorkoutID SERIAL PRIMARY KEY,
                    fldUserID INT NOT NULL,
                    fldWorkoutID INT NOT NULL,
                    FOREIGN KEY (fldUserID) REFERENCES tblUser(fldUserID),
                    FOREIGN KEY (fldWorkoutID) REFERENCES tblWorkout(fldWorkoutID),
                    UNIQUE(fldUserID, fldWorkoutID)
                );";
            createCmd.ExecuteNonQuery();
        }
    }

    private void CreateExerciseTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblExercise (
                    fldExerciseID SERIAL PRIMARY KEY,
                    fldName VARCHAR(100) NOT NULL,
                    fldDescription VARCHAR(100) NOT NULL
                );";
            createCmd.ExecuteNonQuery();
        }
    }

    private void CreateSetTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblSet (
                    fldSetID SERIAL PRIMARY KEY,
                    fldExerciseID INT NOT NULL,
                    fldWeight INT NOT NULL,
                    fldReps INT NOT NULL,
                    fldRestBetweenSet INT NOT NULL,
                    FOREIGN KEY (fldExerciseID) REFERENCES tblExercise(fldExerciseID)
                );";
            createCmd.ExecuteNonQuery();
        }
    }

    private void CreateWorkoutSetTable(NpgsqlConnection connection)
    {
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE tblWorkoutSet (
                    fldWorkoutSetID SERIAL PRIMARY KEY,
                    fldSetID INT NOT NULL,
                    fldWorkoutID INT NOT NULL,
                    FOREIGN KEY (fldSetID) REFERENCES tblSet(fldSetID),
                    FOREIGN KEY (fldWorkoutID) REFERENCES tblWorkout(fldWorkoutID),
                    UNIQUE(fldSetID, fldWorkoutID)
                );";
            createCmd.ExecuteNonQuery();
        }
}
    
    

    public void SeedTestData()
    {
        using var connection = _connectionService.GetConnection();
        connection.Open();

        // Check if test data already exists
        if (!TestDataExists(connection))
        {
            // Create test data in the correct order (respecting foreign keys)
            int credentialsId = CreateTestUserCredentials(connection);
            int userId = CreateTestUser(connection, credentialsId);
            int workoutId = CreateTestWorkout(connection);
            CreateTestUserWorkout(connection, userId, workoutId);
            int exerciseId = CreateTestExercise(connection);
            int setId = CreateTestSet(connection, exerciseId);
            CreateTestWorkoutSet(connection, setId, workoutId);
        }
    }

    private bool TestDataExists(NpgsqlConnection connection)
    {
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = @"
                SELECT COUNT(*) > 0 
                FROM tblUserCredentials 
                WHERE fldUsername = 'test_user';";
            
            return (bool)checkCmd.ExecuteScalar();
        }
    }

    private int CreateTestUserCredentials(NpgsqlConnection connection)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblUserCredentials (fldUsername, fldPassword)
                VALUES (@username, @password)
                RETURNING fldCredentialsID;";
            
            insertCmd.Parameters.AddWithValue("@username", "john_doe");
            insertCmd.Parameters.AddWithValue("@password", "SecurePass123!"); 
            
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }
    }

    private int CreateTestUser(NpgsqlConnection connection, int credentialsId)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblUser (fldCredentialsID, fldName, fldEmail, fldTotalWorkoutTime)
                VALUES (@credentialsId, @name, @email, @workoutTime)
                RETURNING fldUserID;";
            
            insertCmd.Parameters.AddWithValue("@credentialsId", credentialsId);
            insertCmd.Parameters.AddWithValue("@name", "John Doe");
            insertCmd.Parameters.AddWithValue("@email", "john.doe@email.com");
            insertCmd.Parameters.AddWithValue("@workoutTime", 125); // Total workout minutes
            
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }
    }

    private int CreateTestWorkout(NpgsqlConnection connection)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblWorkout (fldDateOfWorkout, fldName)
                VALUES (@date, @name)
                RETURNING fldWorkoutID;";
            
            insertCmd.Parameters.AddWithValue("@date", DateTime.Today.AddDays(-2)); // 2 days ago
            insertCmd.Parameters.AddWithValue("@name", "Morning Chest Day");
            
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }
    }

    private void CreateTestUserWorkout(NpgsqlConnection connection, int userId, int workoutId)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblUserWorkout (fldUserID, fldWorkoutID)
                VALUES (@userId, @workoutId);";
            
            insertCmd.Parameters.AddWithValue("@userId", userId);
            insertCmd.Parameters.AddWithValue("@workoutId", workoutId);
            
            insertCmd.ExecuteNonQuery();
        }
    }

    private int CreateTestExercise(NpgsqlConnection connection)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblExercise (fldName, fldDescription)
                VALUES (@name, @description)
                RETURNING fldExerciseID;";
            
            insertCmd.Parameters.AddWithValue("@name", "Barbell Bench Press");
            insertCmd.Parameters.AddWithValue("@description", "Compound exercise targeting chest, shoulders, and triceps");
            
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }
    }

    private int CreateTestSet(NpgsqlConnection connection, int exerciseId)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblSet (fldExerciseID, fldWeight, fldReps, fldRestBetweenSet)
                VALUES (@exerciseId, @weight, @reps, @rest)
                RETURNING fldSetID;";
            
            insertCmd.Parameters.AddWithValue("@exerciseId", exerciseId);
            insertCmd.Parameters.AddWithValue("@weight", 185); // pounds/kg
            insertCmd.Parameters.AddWithValue("@reps", 10);
            insertCmd.Parameters.AddWithValue("@rest", 90); // seconds
            
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }
    }

    private void CreateTestWorkoutSet(NpgsqlConnection connection, int setId, int workoutId)
    {
        using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO tblWorkoutSet (fldSetID, fldWorkoutID)
                VALUES (@setId, @workoutId);";
            
            insertCmd.Parameters.AddWithValue("@setId", setId);
            insertCmd.Parameters.AddWithValue("@workoutId", workoutId);
            
            insertCmd.ExecuteNonQuery();
        }
    }
}