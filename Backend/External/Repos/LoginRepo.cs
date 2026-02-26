using Backend.Application.Service.Interfaces;
using Npgsql;
using System.Collections.Generic;

namespace Backend.Gateway
{
    public class LoginRepo : ILoginRepo
    {
        private readonly IConnectionService _connectionService;

        public LoginRepo(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public List<string> getCredentials(string username)
        {
            // Handle null or empty username gracefully
            if (string.IsNullOrEmpty(username))
                return new List<string>();

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
}