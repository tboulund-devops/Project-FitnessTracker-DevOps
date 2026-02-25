using Backend.Domain;
using Npgsql;

namespace Backend.External.Repos;

public class UserRepo : IUserRepo
{
    private readonly NpgsqlConnection _connection;

    public UserRepo(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        throw new NotImplementedException("This method is not implemented yet. It should retrieve a user by username from the database.");
    }
    
}