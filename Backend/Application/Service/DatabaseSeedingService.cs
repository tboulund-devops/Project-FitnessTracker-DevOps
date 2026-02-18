using Npgsql;

namespace Backend.Application.Service;

public class DatabaseSeedingService
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

        //Create table if it does not exist
        using (var createCmd = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS ""tblUserCredentials"" (
                ""fldUsername"" VARCHAR(100) NOT NULL,
                ""fldPassword"" VARCHAR(100) NOT NULL
            );
        ", connection))
        {
            createCmd.ExecuteNonQuery();
        }

        //Check if test user already exists
        using (var checkCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) 
            FROM ""tblUserCredentials"" 
            WHERE ""fldUsername"" = @username;
        ", connection))
        {
            checkCmd.Parameters.AddWithValue("@username", "test");

            var count = (long)checkCmd.ExecuteScalar();

            if (count == 0)
            {
                using var insertCmd = new NpgsqlCommand(@"
                    INSERT INTO ""tblUserCredentials"" 
                    (""fldUsername"", ""fldPassword"")
                    VALUES (@username, @password);
                ", connection);

                insertCmd.Parameters.AddWithValue("@username", "test");
                insertCmd.Parameters.AddWithValue("@password", "test");

                insertCmd.ExecuteNonQuery();
            }
        }
    }
}