using Backend.Domain;

namespace UnitTests.Backend_UnitTest.DomainTests;

public class UserInformationDtoTests
{
    [Fact]
    public void Constructor_DefaultValues_AreInitialized()
    {
        var dto = new UserInformationDTO();

        Assert.Equal(string.Empty, dto.Name);
        Assert.Equal(default, dto.TimeOfRegistry);
        Assert.Equal(0, dto.TotalAmountOfWorkouts);
        Assert.Equal("0h 0m", dto.TotalAmountOfTimeTrained);
        Assert.Equal(0, dto.CurrentStreakDays);
        Assert.Equal(0, dto.CurrentStreakWeeks);
        Assert.Equal(0, dto.BestStreakDays);
        Assert.Equal(0, dto.BestStreakWeeks);
        Assert.Equal(string.Empty, dto.FavoriteExercise);
    }

    [Fact]
    public void Properties_CanBeSet_AndReadBack()
    {
        var expectedDate = new DateTime(2025, 10, 20, 10, 30, 0, DateTimeKind.Utc);
        var dto = new UserInformationDTO
        {
            Name = "jane",
            TimeOfRegistry = expectedDate,
            TotalAmountOfWorkouts = 42,
            TotalAmountOfTimeTrained = "14h 35m",
            CurrentStreakDays = 9,
            CurrentStreakWeeks = 1,
            BestStreakDays = 17,
            BestStreakWeeks = 2,
            FavoriteExercise = "Deadlift"
        };

        Assert.Equal("jane", dto.Name);
        Assert.Equal(expectedDate, dto.TimeOfRegistry);
        Assert.Equal(42, dto.TotalAmountOfWorkouts);
        Assert.Equal("14h 35m", dto.TotalAmountOfTimeTrained);
        Assert.Equal(9, dto.CurrentStreakDays);
        Assert.Equal(1, dto.CurrentStreakWeeks);
        Assert.Equal(17, dto.BestStreakDays);
        Assert.Equal(2, dto.BestStreakWeeks);
        Assert.Equal("Deadlift", dto.FavoriteExercise);
    }
}
