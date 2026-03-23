using Backend.Domain;
using Backend.External.Repos;
using Backend.Gateway;
using Backend.Service;
using Moq;

namespace UnitTests.Backend_UnitTest.LoginTests;

public class LoginServiceTests
{
    private readonly Mock<ILoginRepo> _mockRepo;
    private readonly Mock<IUserRepo> _mockUserRepo;
    private readonly LoginService _loginService;

    public LoginServiceTests()
    {
        _mockRepo = new Mock<ILoginRepo>();
        _mockUserRepo = new Mock<IUserRepo>();
        _loginService = new LoginService(_mockRepo.Object, _mockUserRepo.Object);
    }

    [Fact]
    public void Constructor_WithValidRepo_InitializesService()
    {
        var service = new LoginService(_mockRepo.Object, _mockUserRepo.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullRepo_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new LoginService(null!, _mockUserRepo.Object));

        Assert.Equal("repo", exception.ParamName);
    }

    [Fact]
    public void CheckCredentials_NullRequest_ReturnsMinusOne()
    {
        var result = _loginService.CheckCredentials(null!);

        Assert.Equal(-1, result);
        _mockRepo.Verify(x => x.getCredentials(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(x => x.getUserID(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("username", null)]
    [InlineData("username", "")]
    [InlineData("username", "   ")]
    public void CheckCredentials_InvalidPayload_ReturnsMinusOne(string? username, string? password)
    {
        var request = CreateRequest(username, password);

        var result = _loginService.CheckCredentials(request);

        Assert.Equal(-1, result);
        _mockRepo.Verify(x => x.getCredentials(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(x => x.getUserID(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(InvalidCredentialLists))]
    public void CheckCredentials_InvalidRepoCredentialShape_ReturnsMinusOne(List<string>? dbCredentials)
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials!);

        var result = _loginService.CheckCredentials(request);

        Assert.Equal(-1, result);
        _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        _mockRepo.Verify(x => x.getUserID(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CheckCredentials_MismatchedUsername_ReturnsMinusOne()
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockRepo.Setup(x => x.getCredentials(request.Username))
            .Returns(new List<string> { "otheruser", "correctpassword" });

        var result = _loginService.CheckCredentials(request);

        Assert.Equal(-1, result);
        _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        _mockRepo.Verify(x => x.getUserID(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CheckCredentials_MismatchedPassword_ReturnsMinusOne()
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockRepo.Setup(x => x.getCredentials(request.Username))
            .Returns(new List<string> { "testuser", "wrongpassword" });

        var result = _loginService.CheckCredentials(request);

        Assert.Equal(-1, result);
        _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        _mockRepo.Verify(x => x.getUserID(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CheckCredentials_ValidCredentials_ReturnsUserId()
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockRepo.Setup(x => x.getCredentials(request.Username))
            .Returns(new List<string> { "testuser", "correctpassword" });
        _mockRepo.Setup(x => x.getUserID(request.Username)).Returns(42);

        var result = _loginService.CheckCredentials(request);

        Assert.Equal(42, result);
        _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        _mockRepo.Verify(x => x.getUserID(request.Username), Times.Once);
    }

    [Fact]
    public void CheckCredentials_GetCredentialsThrows_PropagatesException()
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockRepo.Setup(x => x.getCredentials(request.Username))
            .Throws(new InvalidOperationException("Database connection failed"));

        var exception = Assert.Throws<InvalidOperationException>(() => _loginService.CheckCredentials(request));

        Assert.Equal("Database connection failed", exception.Message);
        _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        _mockRepo.Verify(x => x.getUserID(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CheckCredentials_GetUserIdThrows_PropagatesException()
    {
        var request = CreateRequest("testuser", "correctpassword");
        _mockRepo.Setup(x => x.getCredentials(request.Username))
            .Returns(new List<string> { "testuser", "correctpassword" });
        _mockRepo.Setup(x => x.getUserID(request.Username))
            .Throws(new InvalidOperationException("User lookup failed"));

        var exception = Assert.Throws<InvalidOperationException>(() => _loginService.CheckCredentials(request));

        Assert.Equal("User lookup failed", exception.Message);
        _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        _mockRepo.Verify(x => x.getUserID(request.Username), Times.Once);
    }

    public static IEnumerable<object?[]> InvalidCredentialLists()
    {
        yield return new object?[] { null };
        yield return new object?[] { new List<string>() };
        yield return new object?[] { new List<string> { "onlyusername" } };
    }

    private static LoginRequest CreateRequest(string? username, string? password) => new()
    {
        Username = username!,
        Password = password!
    };
}