using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.Gateway;

namespace Backend.Service;

public class LoginService : ILoginService
{
    private readonly ILoginRepo _repo;

    public LoginService(ILoginRepo repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    public bool CheckCredentials(LoginRequest request)
    {
        // Validate input
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return false;
        }

        var credentialsFromDatabase = _repo.getCredentials(request.Username);
        
        // Check if we got valid credentials back BEFORE trying to access them
        if (credentialsFromDatabase == null || credentialsFromDatabase.Count < 2)
        {
            return false;
        }

        // Now it's safe to access the list elements
        return credentialsFromDatabase[0] == request.Username &&
               credentialsFromDatabase[1] == request.Password;
    }
}