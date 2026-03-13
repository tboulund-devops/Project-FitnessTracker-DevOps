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

        public List<string> getCredentials(string? username)
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

        public int getUserID(string? username)
        {
            // Handle null or empty username gracefully
            if (string.IsNullOrEmpty(username))
                return -1; // Indicate invalid username

            using var connection = _connectionService.GetConnection();
            connection.Open();

            using var cmd = new NpgsqlCommand(
                @"SELECT u.fldUserID
                  FROM tblUser u
                  INNER JOIN tblUserCredentials c ON c.fldCredentialsID = u.fldCredentialsID
                  WHERE c.fldUsername = @username",
                connection);

            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return reader.GetInt32(0);
            }

            return -1; // Indicate user not found
        }
    }
}