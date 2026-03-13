using Backend.Domain;

namespace Backend.Gateway;

public interface ILoginRepo
{
    public List<string> getCredentials(string? username);
    public int getUserID(string? username);
}