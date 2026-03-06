using Backend.External.Repos;
using Npgsql;

namespace UnitTests.Backend_UnitTest.UserTests;

public class UserRepoTests
{
    // ===================== Constructor Tests =====================

    [Fact]
    public void Constructor_WithValidConnection_InitializesRepo()
    {
        // Arrange
        var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;Username=test;Password=test");

        // Act
        var repo = new UserRepo(connection);

        // Assert
        Assert.NotNull(repo);
    }

    [Fact]
    public void Constructor_WithNullConnection_DoesNotThrow()
    {
        // The constructor does not have a null-check guard
        // Arrange & Act
        var repo = new UserRepo(null!);

        // Assert
        Assert.NotNull(repo);
    }

    // ===================== GetByUsernameAsync Tests =====================

    [Fact]
    public async Task GetByUsernameAsync_AnyUsername_ThrowsNotImplementedException()
    {
        // Arrange
        var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;Username=test;Password=test");
        var repo = new UserRepo(connection);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotImplementedException>(
            () => repo.GetByUsernameAsync("testuser"));
        Assert.Contains("This method is not implemented yet", ex.Message);
    }

    [Fact]
    public async Task GetByUsernameAsync_EmptyUsername_ThrowsNotImplementedException()
    {
        // Arrange
        var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;Username=test;Password=test");
        var repo = new UserRepo(connection);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotImplementedException>(
            () => repo.GetByUsernameAsync(""));
        Assert.Contains("This method is not implemented yet", ex.Message);
    }

    [Fact]
    public async Task GetByUsernameAsync_NullUsername_ThrowsNotImplementedException()
    {
        // Arrange
        var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;Username=test;Password=test");
        var repo = new UserRepo(connection);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotImplementedException>(
            () => repo.GetByUsernameAsync(null!));
        Assert.Contains("This method is not implemented yet", ex.Message);
    }
}

