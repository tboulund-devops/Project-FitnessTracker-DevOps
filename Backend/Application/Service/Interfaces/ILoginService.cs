using Backend.Domain;

namespace Backend.Application.Service.Interfaces;

public interface ILoginService
{
    public int CheckCredentials(LoginRequest request);
    public bool RegisterLoginCredentials(RegisterUserRequest? request);
}