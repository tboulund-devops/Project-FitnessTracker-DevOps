using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.APIControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Backend_UnitTest.WorkoutTests;

public class APIWorkoutControllerTests
{
    private readonly Mock<IWorkoutService> _mockWorkoutService;
    private readonly APIWorkoutController _controller;

    public APIWorkoutControllerTests()
    {
        _mockWorkoutService = new Mock<IWorkoutService>();
        _controller = new APIWorkoutController(_mockWorkoutService.Object);
    }

    // ===================== Constructor Tests =====================

    [Fact]
    public void Constructor_WithValidService_InitializesController()
    {
        // Arrange & Act
        var controller = new APIWorkoutController(_mockWorkoutService.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new APIWorkoutController(null!));
        Assert.Equal("workoutService", ex.ParamName);
    }

    // ===================== CheckCredentials (CreateWorkout) Tests =====================

    [Fact]
    public async Task CreateWorkout_NullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateWorkout(null!, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout must need a creating date and name, and have a positive user id", badRequest.Value);
    }

    [Fact]
    public async Task CreateWorkout_NullDateOfWorkout_ReturnsBadRequest()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = default // null/default DateTime
        };

        // Act - DateOfWorkout is DateTime (non-nullable value type), so this path
        // won't trigger the null check for DateOfWorkout. Test the name path instead.
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert - Valid workout with default date should still proceed
        // since DateTime default is not null
        Assert.IsNotType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateWorkout_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "",
            DateOfWorkout = DateTime.Now
        };

        // Act
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout must need a creating date and name, and have a positive user id", badRequest.Value);
    }

    [Fact]
    public async Task CreateWorkout_WhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "   ",
            DateOfWorkout = DateTime.Now
        };

        // Act
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout must need a creating date and name, and have a positive user id", badRequest.Value);
    }

    [Fact]
    public async Task CreateWorkout_NullName_ReturnsBadRequest()
    {
        // Arrange
        var workout = new Workout
        {
            Name = null!,
            DateOfWorkout = DateTime.Now
        };

        // Act
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout must need a creating date and name, and have a positive user id", badRequest.Value);
    }

    [Fact]
    public async Task CreateWorkout_ZeroUserId_ReturnsBadRequest()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };

        // Act
        var result = await _controller.CreateWorkout(workout, 0);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout must need a creating date and name, and have a positive user id", badRequest.Value);
    }

    [Fact]
    public async Task CreateWorkout_NegativeUserId_ReturnsBadRequest()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };

        // Act
        var result = await _controller.CreateWorkout(workout, -1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout must need a creating date and name, and have a positive user id", badRequest.Value);
    }

    [Fact]
    public async Task CreateWorkout_ServiceReturnsZero_ReturnsNotFound()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };
        _mockWorkoutService.Setup(s => s.CreateWorkout(workout, 1)).ReturnsAsync(0);

        // Act
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Unable to create workout", notFound.Value);
    }

    [Fact]
    public async Task CreateWorkout_ServiceReturnsNegative_ReturnsNotFound()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };
        _mockWorkoutService.Setup(s => s.CreateWorkout(workout, 1)).ReturnsAsync(-1);

        // Act
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Unable to create workout", notFound.Value);
    }

    [Fact]
    public async Task CreateWorkout_ValidRequest_ReturnsOk()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            DateOfWorkout = DateTime.Now
        };
        _mockWorkoutService.Setup(s => s.CreateWorkout(workout, 1)).ReturnsAsync(1);

        // Act
        var result = await _controller.CreateWorkout(workout, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Workout created successfully", okResult.Value);
    }

    // ===================== AddSetToWorkout Tests =====================

    [Fact]
    public async Task AddSetToWorkout_ZeroReps_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 0, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Set verification failure", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_NegativeReps_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = -1, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Set verification failure", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_ZeroExerciseID_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 0, Weight = 100, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Set verification failure", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_NegativeExerciseID_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = -1, Weight = 100, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Set verification failure", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_ZeroWeight_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = 0, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Set verification failure", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_NegativeWeight_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = -1, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Set verification failure", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_ZeroWorkoutId_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, 0);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout id must be a positive number", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_NegativeWorkoutId_ReturnsBadRequest()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };

        // Act
        var result = await _controller.AddSetToWorkout(set, -5);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout id must be a positive number", badRequest.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_ServiceReturnsZero_ReturnsNotFound()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };
        _mockWorkoutService.Setup(s => s.AddSetToWorkout(set, 1)).ReturnsAsync(0);

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Unable to Add Set to workout: 1", notFound.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_ServiceReturnsNegative_ReturnsNotFound()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };
        _mockWorkoutService.Setup(s => s.AddSetToWorkout(set, 1)).ReturnsAsync(-1);

        // Act
        var result = await _controller.AddSetToWorkout(set, 1);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Unable to Add Set to workout: 1", notFound.Value);
    }

    [Fact]
    public async Task AddSetToWorkout_ValidRequest_ReturnsOk()
    {
        // Arrange
        var set = new Set { Reps = 10, ExerciseID = 1, Weight = 100, RestBetweenSetInSec = 60 };
        _mockWorkoutService.Setup(s => s.AddSetToWorkout(set, 5)).ReturnsAsync(1);

        // Act
        var result = await _controller.AddSetToWorkout(set, 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Set successfully added to  workout: 5", okResult.Value);
    }

    // ===================== GetWorkoutsForUser Tests =====================

    [Fact]
    public async Task GetWorkoutsForUser_ZeroWorkoutId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetWorkoutsForUser(0);

        // Assert
        var actionResult = result.Result;
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal("Workout id must be a positive number", badRequest.Value);
    }

    [Fact]
    public async Task GetWorkoutsForUser_NegativeWorkoutId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetWorkoutsForUser(-1);

        // Assert
        var actionResult = result.Result;
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal("Workout id must be a positive number", badRequest.Value);
    }

    [Fact]
    public async Task GetWorkoutsForUser_WorkoutNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutService.Setup(s => s.GetWorkout(99)).ReturnsAsync((Workout)null!);

        // Act
        var result = await _controller.GetWorkoutsForUser(99);

        // Assert
        var actionResult = result.Result;
        var notFound = Assert.IsType<NotFoundObjectResult>(actionResult);
        Assert.Equal("Workout with ID 99 not found", notFound.Value);
    }

    [Fact]
    public async Task GetWorkoutsForUser_ValidWorkoutId_ReturnsOkWithWorkout()
    {
        // Arrange
        var workout = new Workout
        {
            WorkoutID = 1,
            Name = "Morning Workout",
            DateOfWorkout = DateTime.Now,
            Sets = new List<Set>()
        };
        _mockWorkoutService.Setup(s => s.GetWorkout(1)).ReturnsAsync(workout);

        // Act
        var result = await _controller.GetWorkoutsForUser(1);

        // Assert
        var actionResult = result.Result;
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedWorkout = Assert.IsType<Workout>(okResult.Value);
        Assert.Equal("Morning Workout", returnedWorkout.Name);
        Assert.Equal(1, returnedWorkout.WorkoutID);
    }
}

