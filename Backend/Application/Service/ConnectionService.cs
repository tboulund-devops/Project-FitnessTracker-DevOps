using Backend.Application.Service.Interfaces;
using Npgsql;

namespace Backend.Application.Service;

public class ConnectionService :IConnectionService
{
    private readonly string _connectionString;

    public ConnectionService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}