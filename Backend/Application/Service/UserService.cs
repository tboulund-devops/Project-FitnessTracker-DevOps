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

        int currentStreak = 0;
        int bestStreak = 0;
        int streak = 0;

        // Check if current streak is still active (last workout was today or yesterday)
        var today = DateTime.Today;
        var daysSinceLast = (today - sortedDatesDesc[0].Date).Days;
        bool currentStreakActive = daysSinceLast <= 1;

        for (int i = 1; i < sortedDatesDesc.Count; i++)
        {
            var diff = (sortedDatesDesc[i - 1].Date - sortedDatesDesc[i].Date).Days;
            if (diff == 1)
            {
                streak++;
            }
            else
            {
                if (i == 1 || currentStreakActive)
                    currentStreak = streak;
                currentStreakActive = false;
                bestStreak = Math.Max(bestStreak, streak);
                streak = 1;
            }
        }

        bestStreak = Math.Max(bestStreak, streak);
        if (currentStreakActive)
            currentStreak = streak;

        return (currentStreak, bestStreak);
    }
}