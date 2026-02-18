using Backend.Domain;

namespace Backend.Application.Service.Interfaces;

public interface ILoginService
{
    public bool CheckCredentials(LoginRequest request);
}