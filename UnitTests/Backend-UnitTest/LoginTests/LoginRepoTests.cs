// using Backend.Application.Service;
// using Backend.Gateway;
// using Microsoft.AspNetCore.Identity;
// using Npgsql;
// using Xunit;
//
// namespace UnitTests.Backend_UnitTest.LoginTests
// {
//     public class LoginRepoTests : IClassFixture<DatabaseFixture>
//     {
//         private readonly DatabaseFixture _fixture;
//         private readonly LoginRepo _loginRepo;
//
//         public LoginRepoTests(DatabaseFixture fixture)
//         {
//             _fixture = fixture;
//             var connectionService = new ConnectionService("DefaultConnection");
//             _loginRepo = new LoginRepo(connectionService);
//         }
//
//         [Fact]
//         public void GetCredentials_ValidUsername_ReturnsCredentials()
//         {
//             // Arrange
//             var username = "testuser";
//             
//             // Clean up and insert test data
//             CleanupTestData();
//             InsertTestData();
//
//             // Act
//             var result = _loginRepo.getCredentials(username);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(2, result.Count);
//             Assert.Equal("testuser", result[0]);
//             Assert.Equal("password123", result[1]);
//         }
//
//         [Fact]
//         public void GetCredentials_UsernameNotFound_ReturnsEmptyList()
//         {
//             // Arrange
//             var username = "nonexistentuser";
//
//             // Act
//             var result = _loginRepo.getCredentials(username);
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.Empty(result);
//         }
//
//         private void CleanupTestData()
//         {
//             using var connection = _fixture.Connection;
//             connection.Open();
//             using var cmd = new NpgsqlCommand("DELETE FROM tblUserCredentials WHERE fldUsername = 'testuser'", connection);
//             cmd.ExecuteNonQuery();
//         }
//
//         private void InsertTestData()
//         {
//             using var connection = _fixture.Connection;
//             connection.Open();
//             using var cmd = new NpgsqlCommand(
//                 "INSERT INTO tblUserCredentials (fldUsername, fldPassword) VALUES ('testuser', 'password123')", 
//                 connection);
//             cmd.ExecuteNonQuery();
//         }
//     }
//
//     public class DatabaseFixture : IDisposable
//     {
//         public NpgsqlConnection Connection { get; private set; }
//
//         public DatabaseFixture()
//         {
//             // Use a test database connection string
//             var connectionString = "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpass;";
//             Connection = new NpgsqlConnection(connectionString);
//         }
//
//         public void Dispose()
//         {
//             Connection?.Dispose();
//         }
//     }
// }