using Backend.Application.Service;
using Backend.Domain;
using Backend.External.Repos.Interface;
using Moq;

namespace UnitTests.Backend_UnitTest.WorkoutTests;

public class WorkoutServiceTests
{
    private readonly Mock<IWorkoutRepo> _mockRepo;
    private readonly WorkoutService _workoutService;

    public WorkoutServiceTests()
    {
        _mockRepo = new Mock<IWorkoutRepo>();
        _workoutService = new WorkoutService(_mockRepo.Object);
    }

    // ===================== Constructor Tests =====================

    [Fact]
    public void Constructor_WithValidRepo_InitializesService()
    {
        // Arrange & Act
        var service = new WorkoutService(_mockRepo.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullRepo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new WorkoutService(null!));
        Assert.Equal("repo", ex.ParamName);
    }

    // ===================== CreateWorkout Tests =====================

    [Fact]
    public async Task CreateWorkout_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "",
            DateOfWorkout = DateTime.Now
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkout(workout, 1));
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public async Task CreateWorkout_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "   ",
            DateOfWorkout = DateTime.Now
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkout(workout, 1));
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public async Task CreateWorkout_NullName_ThrowsArgumentException()
    {
        // Arrange
        var workout = new Workout
        {
            Name = null!,
            DateOfWorkout = DateTime.Now
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkout(workout, 1));
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public async Task CreateWorkout_ZeroUserId_ThrowsArgumentException()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkout(workout, 0));
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public async Task CreateWorkout_NegativeUserId_ThrowsArgumentException()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkout(workout, -1));
        Assert.Equal("Name is required", ex.Message);
    }

    [Fact]
    public async Task CreateWorkout_ValidRequest_ReturnsRepoResult()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };
        _mockRepo.Setup(r => r.CreateWorkout(workout, 1)).ReturnsAsync(42);

        // Act
        var result = await _workoutService.CreateWorkout(workout, 1);

        // Assert
        Assert.Equal(42, result);
        _mockRepo.Verify(r => r.CreateWorkout(workout, 1), Times.Once);
    }

    // ===================== AddSetToWorkout Tests =====================

    [Fact]
    public async Task AddSetToWorkout_NullSet_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.AddSetToWorkout(null!, 1));
        Assert.Equal("Set verification failed", ex.Message);
    }

    [Fact]
    public async Task AddSetToWorkout_ZeroWorkoutId_ThrowsArgumentException()
    {
        // Arrange
        var set = new Set { ExerciseID = 1, Weight = 100, Reps = 10, RestBetweenSetInSec = 60 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.AddSetToWorkout(set, 0));
        Assert.Equal("Set verification failed", ex.Message);
    }

    [Fact]
    public async Task AddSetToWorkout_NegativeWorkoutId_ThrowsArgumentException()
    {
        // Arrange
        var set = new Set { ExerciseID = 1, Weight = 100, Reps = 10, RestBetweenSetInSec = 60 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.AddSetToWorkout(set, -1));
        Assert.Equal("Set verification failed", ex.Message);
    }

    [Fact]
    public async Task AddSetToWorkout_ValidRequest_ReturnsRepoResult()
    {
        // Arrange
        var set = new Set { ExerciseID = 1, Weight = 100, Reps = 10, RestBetweenSetInSec = 60 };
        _mockRepo.Setup(r => r.AddSetToWorkout(set, 5)).ReturnsAsync(7);

        // Act
        var result = await _workoutService.AddSetToWorkout(set, 5);

        // Assert
        Assert.Equal(7, result);
        _mockRepo.Verify(r => r.AddSetToWorkout(set, 5), Times.Once);
    }

    // ===================== GetWorkout Tests =====================

    [Fact]
    public async Task GetWorkout_ZeroId_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.GetWorkout(0));
        Assert.Equal("Workout ID is required", ex.Message);
    }

    [Fact]
    public async Task GetWorkout_NegativeId_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.GetWorkout(-1));
        Assert.Equal("Workout ID is required", ex.Message);
    }

    [Fact]
    public async Task GetWorkout_ValidId_ReturnsWorkoutFromRepo()
    {
        // Arrange
        var expectedWorkout = new Workout
        {
            WorkoutID = 1,
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now,
            Sets = new List<Set>()
        };
        _mockRepo.Setup(r => r.getWorkout(1)).ReturnsAsync(expectedWorkout);

        // Act
        var result = await _workoutService.GetWorkout(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedWorkout.WorkoutID, result.WorkoutID);
        Assert.Equal(expectedWorkout.Name, result.Name);
        _mockRepo.Verify(r => r.getWorkout(1), Times.Once);
    }

    [Fact]
    public async Task GetWorkout_ValidId_RepoReturnsNull_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.getWorkout(999)).ReturnsAsync((Workout)null!);

        // Act
        var result = await _workoutService.GetWorkout(999);

        // Assert
        Assert.Null(result);
        _mockRepo.Verify(r => r.getWorkout(999), Times.Once);
    }
}

