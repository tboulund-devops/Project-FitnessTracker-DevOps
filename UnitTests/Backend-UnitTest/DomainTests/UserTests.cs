using Backend.Domain;

namespace UnitTests.Backend_UnitTest.DomainTests;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange & Act
        var user = new User(1, "testuser", "test@example.com");

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void Constructor_WithZeroId_SetsId()
    {
        // Arrange & Act
        var user = new User(0, "testuser", "test@example.com");

        // Assert
        Assert.Equal(0, user.Id);
    }

    [Fact]
    public void Constructor_WithNegativeId_SetsId()
    {
        // Arrange & Act
        var user = new User(-1, "testuser", "test@example.com");

        // Assert
        Assert.Equal(-1, user.Id);
    }

    [Fact]
    public void Constructor_WithNullUsername_SetsNull()
    {
        // Arrange & Act
        var user = new User(1, null!, null!);

        // Assert
        Assert.Null(user.Username);
        Assert.Null(user.Email);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_SetsEmptyStrings()
    {
        // Arrange & Act
        var user = new User(1, "", "");

        // Assert
        Assert.Equal("", user.Username);
        Assert.Equal("", user.Email);
    }

    [Fact]
    public void Id_SetProperty_UpdatesValue()
    {
        // Arrange
        var user = new User(1, "testuser", "test@example.com");

        // Act
        user.Id = 42;

        // Assert
        Assert.Equal(42, user.Id);
    }

    [Fact]
    public void Username_SetProperty_UpdatesValue()
    {
        // Arrange
        var user = new User(1, "testuser", "test@example.com");

        // Act
        user.Username = "newuser";

        // Assert
        Assert.Equal("newuser", user.Username);
    }

    [Fact]
    public void Email_SetProperty_UpdatesValue()
    {
        // Arrange
        var user = new User(1, "testuser", "test@example.com");

        // Act
        user.Email = "new@example.com";

        // Assert
        Assert.Equal("new@example.com", user.Email);
    }
}

