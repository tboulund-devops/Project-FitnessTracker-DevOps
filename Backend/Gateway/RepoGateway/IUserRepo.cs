using Backend.Domain;

namespace Backend.External.Repos;

public interface IUserRepo
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetUserInfoByIdAsync(int userId);
    Task<int> GetWorkoutCountAsync(int userId);
    Task<List<DateTime>> GetWorkoutDatesAsync(int userId);
    Task<string?> GetMostUsedExerciseAsync(int userId);
    bool AddUserInformation(int credentialsId, string? name, string? email, int totalWorkoutTime);
    Task <bool> UpdateUserEmailAsync(int userId, string newEmail);
}
