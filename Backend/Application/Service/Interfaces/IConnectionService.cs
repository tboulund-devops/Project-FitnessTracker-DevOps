using Npgsql;

namespace Backend.Application.Service.Interfaces;

public interface IConnectionService
{
    public NpgsqlConnection GetConnection();
}