using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.APIControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Backend_UnitTest.LoginTests;

public class APILoginControllerTest
{
    private readonly Mock<ILoginService> _mockLoginService = new();
    private readonly APILoginController _controller;

    public APILoginControllerTest()
    {
        _controller = new APILoginController(_mockLoginService.Object);
    }

    [Fact]
    public void Constructor_WithValidLoginService_InitializesController()
    {
        var controller = new APILoginController(_mockLoginService.Object);

        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullLoginService_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new APILoginController(null!));

        Assert.Equal("loginService", exception.ParamName);
    }

    [Fact]
    public void CheckCredentials_NullRequest_ReturnsUnauthorized()
    {
        ActionResult<int> result = _controller.CheckCredentials(null);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
        _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("username", null)]
    [InlineData("username", "")]
    [InlineData("username", "   ")]
    public void CheckCredentials_InvalidPayload_ReturnsUnauthorized(string? username, string? password)
    {
        var request = CreateRequest(username, password);

        ActionResult<int> result = _controller.CheckCredentials(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
        _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CheckCredentials_InvalidCredentials_ReturnsUnauthorized(int returnedUserId)
    {
        var request = CreateRequest("testuser", "wrongpassword");
        _mockLoginService.Setup(x => x.CheckCredentials(request)).Returns(returnedUserId);

        ActionResult<int> result = _controller.CheckCredentials(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        _mockLoginService.Verify(x => x.CheckCredentials(request), Times.Once);
    }

    [Fact]
    public void CheckCredentials_ValidCredentials_ReturnsOkWithUserId()
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockLoginService.Setup(x => x.CheckCredentials(request)).Returns(42);

        ActionResult<int> result = _controller.CheckCredentials(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(42, okResult.Value);
        _mockLoginService.Verify(x => x.CheckCredentials(request), Times.Once);
    }

    [Fact]
    public void CheckCredentials_LoginServiceThrowsException_PropagatesException()
    {
        var request = CreateRequest("testuser", "password");
        _mockLoginService.Setup(x => x.CheckCredentials(request))
            .Throws(new InvalidOperationException("Database connection failed"));

        var exception = Assert.Throws<InvalidOperationException>(() => _controller.CheckCredentials(request));

        Assert.Equal("Database connection failed", exception.Message);
        _mockLoginService.Verify(x => x.CheckCredentials(request), Times.Once);
    }

    [Fact]
    public void RegisterLoginCredentials_NullRequest_ReturnsBadRequest()
    {
        IActionResult result = _controller.RegisterLoginCredentials(null);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required", badRequestResult.Value);
        _mockLoginService.Verify(x => x.RegisterLoginCredentials(It.IsAny<RegisterUserRequest>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "password", "Name", "email@test.com", 0)]
    [InlineData("", "password", "Name", "email@test.com", 0)]
    [InlineData("username", null, "Name", "email@test.com", 0)]
    [InlineData("username", "password", "", "email@test.com", 0)]
    [InlineData("username", "password", "Name", "", 0)]
    public void RegisterLoginCredentials_MissingRequiredFields_ReturnsBadRequest(
        string? username,
        string? password,
        string? name,
        string? email,
        int totalWorkoutTime)
    {
        var request = CreateRegisterRequest(username, password, name, email, totalWorkoutTime);

        IActionResult result = _controller.RegisterLoginCredentials(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username, password, name and email are required", badRequestResult.Value);
        _mockLoginService.Verify(x => x.RegisterLoginCredentials(It.IsAny<RegisterUserRequest>()), Times.Never);
    }

    [Fact]
    public void RegisterLoginCredentials_NegativeWorkoutTime_ReturnsConflict()
    {
        var request = CreateRegisterRequest("username", "password", "Name", "email@test.com", -1);

        IActionResult result = _controller.RegisterLoginCredentials(request);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Registration failed. Username may already exist", conflictResult.Value);
        _mockLoginService.Verify<bool>(x => x.RegisterLoginCredentials(It.IsAny<RegisterUserRequest>()), Times.Once());
    }

    [Fact]
    public void RegisterLoginCredentials_ServiceReturnsFalse_ReturnsConflict()
    {
        var request = CreateRegisterRequest("username", "password", "Name", "email@test.com", 0);
        _mockLoginService
            .Setup(x => x.RegisterLoginCredentials(It.Is<RegisterUserRequest>(r => r.Username == "username" && r.Password == "password")))
            .Returns(false);

        IActionResult result = _controller.RegisterLoginCredentials(request);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Registration failed. Username may already exist", conflictResult.Value);
        _mockLoginService.Verify<bool>(
            x => x.RegisterLoginCredentials(It.Is<RegisterUserRequest>(r => r.Username == "username" && r.Password == "password")),
            Times.Once());
    }

    [Fact]
    public void RegisterLoginCredentials_ServiceReturnsTrue_ReturnsOk()
    {
        var request = CreateRegisterRequest("username", "password", "Name", "email@test.com", 0);
        _mockLoginService
            .Setup(x => x.RegisterLoginCredentials(It.Is<RegisterUserRequest>(r => r.Username == "username" && r.Password == "password")))
            .Returns(true);

        IActionResult result = _controller.RegisterLoginCredentials(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Credentials registered. User profile fields received", okResult.Value);
        _mockLoginService.Verify<bool>(
            x => x.RegisterLoginCredentials(It.Is<RegisterUserRequest>(r => r.Username == "username" && r.Password == "password")),
            Times.Once());
    }

    private static LoginRequest CreateRequest(string? username, string? password) => new()
    {
        Username = username!,
        Password = password!
    };

    private static RegisterUserRequest CreateRegisterRequest(
        string? username,
        string? password,
        string? name,
        string? email,
        int totalWorkoutTime) => new()
    {
        Username = username!,
        Password = password!,
        Name = name!,
        Email = email!,
        TotalWorkoutTime = totalWorkoutTime
    };
}