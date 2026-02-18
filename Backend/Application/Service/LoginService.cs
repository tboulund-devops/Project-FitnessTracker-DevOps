


using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.Gateway;


namespace Backend.Service;

public class LoginService:ILoginService
{
    private readonly ILoginRepo _repo;

    public LoginService(ILoginRepo repo)
    {
        _repo = repo;
    }

    public bool CheckCredentials(LoginRequest request)
    {
        var CredentialsFromDatabase = _repo.getCredentials(request.Username);
        
        if (CredentialsFromDatabase[0] == request.Username &&
            CredentialsFromDatabase[1] == request.Password)
        {
            return true;
        }

        return false;
    }
}