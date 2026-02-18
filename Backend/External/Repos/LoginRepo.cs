namespace DefaultNamespace;

public class LoginRepo : ILoginRepo
{
    private readonly IPostgressqlDatabase _database;

    public LoginRepo(IPostgressqlDatabase database)
    {
        _database = database;
    }
    public Director<string, string> getCredentials(string username, string password)
    {
        
    }
}