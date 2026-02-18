


using Backend.Domain;
using Backend.Gateway;


namespace Backend.Service;

public class LoginService
{
    private readonly ILoginRepo _repo;

    public LoginService(ILoginRepo repo)
    {
        _repo = repo;
    }

    public bool CheckCredentials(LoginRequest request)
    {
        var CredentialsFromDatabase = _repo.getCredentials(request.Username);

        if (CredentialsFromDatabase.ContainsKey(request.Username) &&
            CredentialsFromDatabase.ContainsValue(request.Password))
        {
            return true;
        }
        return false;
    }
}