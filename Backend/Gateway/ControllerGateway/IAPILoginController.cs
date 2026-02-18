

using Backend.Domain;

namespace DefaultNamespace;

public interface IAPILoginController
{
    public bool CheckCredentials(LoginRequest request);
}