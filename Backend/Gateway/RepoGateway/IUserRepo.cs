using Backend.Domain;

namespace Backend.External.Repos;

public interface IUserRepo
{
    Task<User?> GetByUsernameAsync(string username);
}