using Backend.Domain;

namespace Backend.Gateway;

public interface ILoginRepo
{
    public Dictionary<string, string> getCredentials(string username);
}