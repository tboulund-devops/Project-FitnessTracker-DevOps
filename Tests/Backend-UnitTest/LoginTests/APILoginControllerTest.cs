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

    private static LoginRequest CreateRequest(string? username, string? password) => new()
    {
        Username = username!,
        Password = password!
    };
}