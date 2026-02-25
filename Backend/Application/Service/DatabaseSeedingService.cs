using Npgsql;
using System.Data;
using Backend.Application.Service.Interfaces;

namespace Backend.Application.Service;

public class DatabaseSeedingService :IDatabaseSeedingService
{
    private readonly ConnectionService _connectionService;

    public DatabaseSeedingService(ConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public void Seed()
    {
        using var connection = _connectionService.GetConnection();
        connection.Open();

        // Create table with unquoted identifiers
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS tblUserCredentials (
                    fldUsername VARCHAR(100) NOT NULL,
                    fldPassword VARCHAR(100) NOT NULL
                );";
            createCmd.ExecuteNonQuery();
        }

        // Check if test user already exists
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = @"
                SELECT COUNT(*) 
                FROM tblUserCredentials 
                WHERE fldUsername = @username;";
            
            var parameter = checkCmd.CreateParameter();
            parameter.ParameterName = "@username";
            parameter.Value = "test";
            checkCmd.Parameters.Add(parameter);

            var count = (long?)checkCmd.ExecuteScalar() ?? 0;

            if (count == 0)
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO tblUserCredentials 
                    (fldUsername, fldPassword)
                    VALUES (@username, @password);";

                var usernameParam = insertCmd.CreateParameter();
                usernameParam.ParameterName = "@username";
                usernameParam.Value = "test";
                insertCmd.Parameters.Add(usernameParam);

                var passwordParam = insertCmd.CreateParameter();
                passwordParam.ParameterName = "@password";
                passwordParam.Value = "test";
                insertCmd.Parameters.Add(passwordParam);

                insertCmd.ExecuteNonQuery();
            }
        }
    }
}