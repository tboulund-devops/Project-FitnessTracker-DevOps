using Backend.Domain;
using Backend.External.Repos;
using Backend.Service;
using Moq;

namespace UnitTests.Backend_UnitTest.UserTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepo> _mockUserRepo;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepo>();
        _userService = new UserService(_mockUserRepo.Object);
    }

    // ===================== Constructor Tests =====================

    [Fact]
    public void Constructor_WithValidRepo_InitializesService()
    {
        // Arrange & Act
        var service = new UserService(_mockUserRepo.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullRepo_DoesNotThrow()
    {
        // The constructor does not have a null-check, so this should not throw
        // Arrange & Act
        var service = new UserService(null!);

        // Assert
        Assert.NotNull(service);
    }

    // ===================== GetUserByUsername Tests =====================

    [Fact]
    public async Task GetUserByUsername_UserExists_ReturnsUser()
    {
        // Arrange
        var expectedUser = new User(1, "testuser", "test@example.com");
        _mockUserRepo.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByUsername("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("testuser"), Times.Once);
    }

    [Fact]
    public async Task GetUserByUsername_UserNotFound_ReturnsNull()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByUsernameAsync("nonexistent")).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByUsername("nonexistent");

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync("nonexistent"), Times.Once);
    }

    [Fact]
    public async Task GetUserByUsername_EmptyUsername_ReturnsNull()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByUsernameAsync("")).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByUsername("");

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync(""), Times.Once);
    }

    [Fact]
    public async Task GetUserByUsername_NullUsername_ReturnsNull()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByUsernameAsync(null!)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByUsername(null!);

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetByUsernameAsync(null!), Times.Once);
    }

    // ===================== GetUserInformation Tests =====================

    [Fact]
    public async Task GetUserInformation_UserNotFound_ReturnsNull()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserInfoByIdAsync(10)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserInformation(10);

        // Assert
        Assert.Null(result);
        _mockUserRepo.Verify(r => r.GetUserInfoByIdAsync(10), Times.Once);
        _mockUserRepo.Verify(r => r.GetWorkoutCountAsync(It.IsAny<int>()), Times.Never);
        _mockUserRepo.Verify(r => r.GetWorkoutDatesAsync(It.IsAny<int>()), Times.Never);
        _mockUserRepo.Verify(r => r.GetMostUsedExerciseAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserInformation_NoWorkoutDates_UsesDefaultsAndFallbackExercise()
    {
        // Arrange
        var registration = new DateTime(2024, 01, 01, 12, 00, 00, DateTimeKind.Utc);
        var user = new User(3, "jane", "jane@example.com")
        {
            TimeOfRegistration = registration,
            TotalWorkoutTimeMinutes = 125
        };

        _mockUserRepo.Setup(r => r.GetUserInfoByIdAsync(3)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetWorkoutCountAsync(3)).ReturnsAsync(4);
        _mockUserRepo.Setup(r => r.GetWorkoutDatesAsync(3)).ReturnsAsync(new List<DateTime>());
        _mockUserRepo.Setup(r => r.GetMostUsedExerciseAsync(3)).ReturnsAsync((string?)null);

        // Act
        var result = await _userService.GetUserInformation(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jane", result.Name);
        Assert.Equal(registration, result.TimeOfRegistry);
        Assert.Equal(4, result.TotalAmountOfWorkouts);
        Assert.Equal("2h 5m", result.TotalAmountOfTimeTrained);
        Assert.Equal(0, result.CurrentStreakDays);
        Assert.Equal(0, result.CurrentStreakWeeks);
        Assert.Equal(0, result.BestStreakDays);
        Assert.Equal(0, result.BestStreakWeeks);
        Assert.Equal("N/A", result.FavoriteExercise);
    }

    [Fact]
    public async Task GetUserInformation_ActiveStreak_CalculatesCurrentAndBestStreak()
    {
        // Arrange
        var today = DateTime.Today;
        var user = new User(9, "athlete", "a@example.com")
        {
            TimeOfRegistration = today.AddMonths(-2),
            TotalWorkoutTimeMinutes = 60
        };

        var sortedDatesDesc = new List<DateTime>
        {
            today,
            today.AddDays(-1),
            today.AddDays(-2),
            today.AddDays(-4)
        };

        _mockUserRepo.Setup(r => r.GetUserInfoByIdAsync(9)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetWorkoutCountAsync(9)).ReturnsAsync(7);
        _mockUserRepo.Setup(r => r.GetWorkoutDatesAsync(9)).ReturnsAsync(sortedDatesDesc);
        _mockUserRepo.Setup(r => r.GetMostUsedExerciseAsync(9)).ReturnsAsync("Squat");

        // Act
        var result = await _userService.GetUserInformation(9);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1h 0m", result.TotalAmountOfTimeTrained);
        Assert.Equal(2, result.CurrentStreakDays);
        Assert.Equal(0, result.CurrentStreakWeeks);
        Assert.Equal(2, result.BestStreakDays);
        Assert.Equal(0, result.BestStreakWeeks);
        Assert.Equal("Squat", result.FavoriteExercise);
    }

    [Fact]
    public async Task GetUserInformation_InactiveStreak_TracksBestWithoutUpdatingCurrentAfterBreak()
    {
        // Arrange
        var today = DateTime.Today;
        var user = new User(11, "inactive", "i@example.com")
        {
            TimeOfRegistration = today.AddYears(-1),
            TotalWorkoutTimeMinutes = 59
        };

        var sortedDatesDesc = new List<DateTime>
        {
            today.AddDays(-3),
            today.AddDays(-5),
            today.AddDays(-7)
        };

        _mockUserRepo.Setup(r => r.GetUserInfoByIdAsync(11)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetWorkoutCountAsync(11)).ReturnsAsync(3);
        _mockUserRepo.Setup(r => r.GetWorkoutDatesAsync(11)).ReturnsAsync(sortedDatesDesc);
        _mockUserRepo.Setup(r => r.GetMostUsedExerciseAsync(11)).ReturnsAsync("Row");

        // Act
        var result = await _userService.GetUserInformation(11);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0h 59m", result.TotalAmountOfTimeTrained);
        Assert.Equal(0, result.CurrentStreakDays);
        Assert.Equal(1, result.BestStreakDays);
    }

    [Fact]
    public async Task GetUserInformation_ContinuousActiveStreak_UpdatesCurrentAtLoopEnd()
    {
        var today = DateTime.Today;
        var user = new User(12, "consistent", "c@example.com")
        {
            TimeOfRegistration = today.AddMonths(-1),
            TotalWorkoutTimeMinutes = 121
        };

        var sortedDatesDesc = new List<DateTime>
        {
            today,
            today.AddDays(-1),
            today.AddDays(-2)
        };

        _mockUserRepo.Setup(r => r.GetUserInfoByIdAsync(12)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetWorkoutCountAsync(12)).ReturnsAsync(3);
        _mockUserRepo.Setup(r => r.GetWorkoutDatesAsync(12)).ReturnsAsync(sortedDatesDesc);
        _mockUserRepo.Setup(r => r.GetMostUsedExerciseAsync(12)).ReturnsAsync("Bench");

        var result = await _userService.GetUserInformation(12);

        Assert.NotNull(result);
        Assert.Equal("2h 1m", result.TotalAmountOfTimeTrained);
        Assert.Equal(2, result.CurrentStreakDays);
        Assert.Equal(2, result.BestStreakDays);
        Assert.Equal("Bench", result.FavoriteExercise);
    }
}
