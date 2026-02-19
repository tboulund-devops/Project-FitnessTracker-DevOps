namespace Backend.External.Repos.Interface;

public interface ILoginRepo
{
    public List<string> getCredentials(string username);
}