using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.APIControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Backend_UnitTest.UserTests;

public class ApiUserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly APIUserController _controller;

    public ApiUserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new APIUserController(_mockUserService.Object);
    }

    [Fact]
    public void Constructor_WithValidService_InitializesController()
    {
        var controller = new APIUserController(_mockUserService.Object);

        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new APIUserController(null!));

        Assert.Equal("userService", ex.ParamName);
    }

    [Fact]
    public async Task GetWorkoutsForUser_WithNonPositiveUserId_ReturnsBadRequest()
    {
        var result = await _controller.GetWorkoutsForUser(0);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User id must be a positive number", badRequest.Value);
        _mockUserService.Verify(s => s.GetUserInformation(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetWorkoutsForUser_UserNotFound_ReturnsNotFound()
    {
        _mockUserService.Setup(s => s.GetUserInformation(5)).ReturnsAsync((UserInformationDTO?)null);

        var result = await _controller.GetWorkoutsForUser(5);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User with ID 5 not found", notFound.Value);
        _mockUserService.Verify(s => s.GetUserInformation(5), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutsForUser_UserFound_ReturnsOkWithDto()
    {
        var dto = new UserInformationDTO
        {
            Name = "Jakob",
            TotalAmountOfWorkouts = 12,
            TotalAmountOfTimeTrained = "8h 20m"
        };

        _mockUserService.Setup(s => s.GetUserInformation(7)).ReturnsAsync(dto);

        var result = await _controller.GetWorkoutsForUser(7);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<UserInformationDTO>(ok.Value);
        Assert.Equal("Jakob", payload.Name);
        Assert.Equal(12, payload.TotalAmountOfWorkouts);
        Assert.Equal("8h 20m", payload.TotalAmountOfTimeTrained);
        _mockUserService.Verify(s => s.GetUserInformation(7), Times.Once);
    }
}
