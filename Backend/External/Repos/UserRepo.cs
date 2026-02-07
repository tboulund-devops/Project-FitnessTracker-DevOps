using Backend.Domain;

namespace Backend.External.Repos;

public class UserRepo : IUserRepo
{
    private readonly string _context;

    public UserRepo(string context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        throw new NotImplementedException("This method is not implemented yet. It should retrieve a user by username from the database.");
    }
}