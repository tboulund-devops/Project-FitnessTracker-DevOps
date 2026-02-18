using Backend.Application.Service;
using Backend.Domain;
using Backend.Gateway;
using Npgsql;

public class LoginRepo : ILoginRepo
{
    private readonly ConnectionService _connectionService;

    public LoginRepo(ConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public Dictionary<string, string> getCredentials(string username)
    {
        using var connection = _connectionService.GetConnection();
        connection.Open();

        using var cmd = new NpgsqlCommand(
            "SELECT fldusername, fldpassword FROM tblusercredentials WHERE username = @username",
            connection);

        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();

        var result = new Dictionary<string, string>();

        if (reader.Read())
        {
            result["username"] = reader.GetString(0);
            result["password"] = reader.GetString(1);
        }

        return result;
    }
}