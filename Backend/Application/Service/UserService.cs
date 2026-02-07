using Backend.Domain;
using Backend.External;
using Backend.External.Repos;

namespace Backend.Service;

public class UserService
{
    private readonly IUserRepo _userRepo;

    public UserService(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        var user = await _userRepo.GetByUsernameAsync(username);

        if (user != null) return user;
        return null; // User not found
    }
}