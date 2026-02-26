using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.Gateway;
using Microsoft.AspNetCore.Identity;
using Npgsql;

public class LoginRepo : ILoginRepo
{
    private readonly IConnectionService _connectionService;

    public LoginRepo(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public List<string> getCredentials(string username)
    {
        using var connection = _connectionService.GetConnection();
        connection.Open();

        using var cmd = new NpgsqlCommand(
            "SELECT fldUsername, fldPassword FROM tblUserCredentials WHERE fldUsername = @username",
            connection);

        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();

        var result = new List<string>();

        if (reader.Read())
        {
            result.Add(reader.GetString(0).Trim());
            result.Add(reader.GetString(1).Trim());
        }
        
        return result;
    }
}