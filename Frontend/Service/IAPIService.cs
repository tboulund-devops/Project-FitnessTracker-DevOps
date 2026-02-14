using System.Threading.Tasks;

namespace FitnessTracker.UI.Service;

public interface IAPIService
{
    /// <summary>
    /// Authenticates user
    /// </summary>
    Task<bool> LoginAsync(string username, string password);
    
}