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
}

