using System.Threading.Tasks;

namespace Frontend.Service;

public interface IAPIService
{
    /// <summary>
    /// Authenticates user
    /// </summary>
    Task<bool> LoginAsync(string username, string password);
    
}