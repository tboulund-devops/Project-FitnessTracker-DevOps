namespace DefaultNamespace;

public interface ILoginRepo
{
    public Director<string, string> getCredentials(string username, string password);
}