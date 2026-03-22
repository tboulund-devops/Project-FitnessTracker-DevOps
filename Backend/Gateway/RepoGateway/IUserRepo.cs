using Backend.Domain;

namespace Backend.External.Repos;

public interface IUserRepo
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetUserInfoByIdAsync(int userId);
    Task<int> GetWorkoutCountAsync(int userId);
    Task<List<DateTime>> GetWorkoutDatesAsync(int userId);
    Task<string?> GetMostUsedExerciseAsync(int userId);
    bool addUserInformation(int credentialsId, string? name, string? email, int totalWorkoutTime);
}
