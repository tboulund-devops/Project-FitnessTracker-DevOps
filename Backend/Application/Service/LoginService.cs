namespace DefaultNamespace;

public class LoginService
{
    private readonly ILoginRepo _repo;

    public LoginService(ILoginRepo repo)
    {
        _repo = repo;
    }

    public bool CheckCredentials(string username, string password)
    {
        var CredentialsFromDatabase = _repo.getCredentials(username, password);
    }
}