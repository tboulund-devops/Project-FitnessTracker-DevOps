using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.Repos;

namespace Backend.Service;

public class UserService : IUserService
{
    private readonly IUserRepo _userRepo;

    public UserService(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _userRepo.GetByUsernameAsync(username);
    }

    public async Task<UserInformationDTO?> GetUserInformation(int userID)
    {
        var user = await _userRepo.GetUserInfoByIdAsync(userID);
        if (user == null)
            return null;

        var totalWorkouts = await _userRepo.GetWorkoutCountAsync(userID);
        var workoutDates = await _userRepo.GetWorkoutDatesAsync(userID);
        var favoriteExercise = await _userRepo.GetMostUsedExerciseAsync(userID);

        var (currentStreak, bestStreak) = CalculateStreaks(workoutDates);
        var formattedTime = FormatWorkoutTime(user.TotalWorkoutTimeMinutes);

        return new UserInformationDTO
        {
            Name = user.Username,
            TimeOfRegistry = user.TimeOfRegistration,
            TotalAmountOfWorkouts = totalWorkouts,
            TotalAmountOfTimeTrained = formattedTime,
            CurrentStreakDays = currentStreak,
            CurrentStreakWeeks = currentStreak / 7,
            BestStreakDays = bestStreak,
            BestStreakWeeks = bestStreak / 7,
            FavoriteExercise = favoriteExercise ?? "N/A"
        };
    }
    
    public async Task<User> GetProfileInfo(int userID)
    {
        var user = await _userRepo.GetUserInfoByIdAsync(userID);
        if (user == null)
            throw new Exception($"User with ID {userID} not found");

        return user;
    }
    
    public async Task<bool> UpdateUserEmail(int userID, string newEmail)
    {
        // Lazily create FeatureHub dependency so unrelated service calls/tests do not block on external network init.
        var featureStateProvider = new FeaturehubHelper.FeatureStateProvider();

        //Backend check for feature toggle - if the feature is disabled, throw an exception or return an error response
        if (!featureStateProvider.IsEnabled("ChangeEmail"))
        {
            throw new Exception("ChangeEmail feature is disabled");
        }

        var user = await _userRepo.GetUserInfoByIdAsync(userID);
        if (user == null)
            throw new Exception($"User with ID {userID} not found");

        return await _userRepo.UpdateUserEmailAsync(userID, newEmail);
    }

    private static string FormatWorkoutTime(int totalMinutes)
    {
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours}h {minutes}m";
    }

    private static (int currentStreak, int bestStreak) CalculateStreaks(List<DateTime> sortedDatesDesc)
    {
        if (sortedDatesDesc.Count == 0)
            return (0, 0);

        var today = DateTime.Today;
        var daysSinceLast = (today - sortedDatesDesc[0].Date).Days;
        bool isCurrentStreakActive = daysSinceLast <= 1;

        int currentStreak = 0;
        int bestStreak = 0;
        int runningStreak = 1;

        for (int i = 1; i < sortedDatesDesc.Count; i++)
        {
            var daysBetween = (sortedDatesDesc[i - 1].Date - sortedDatesDesc[i].Date).Days;
            
            if (daysBetween == 1)
            {
                runningStreak++;
            }
            else
            {
                if (i == 1 && isCurrentStreakActive)
                    currentStreak = runningStreak;
                    
                bestStreak = Math.Max(bestStreak, runningStreak);
                runningStreak = 1;
                isCurrentStreakActive = false;
            }
        }

        bestStreak = Math.Max(bestStreak, runningStreak);
        if (isCurrentStreakActive)
            currentStreak = runningStreak;

        return (currentStreak, bestStreak);
    }
    
    
}