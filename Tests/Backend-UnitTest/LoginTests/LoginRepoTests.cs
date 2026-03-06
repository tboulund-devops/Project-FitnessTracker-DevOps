using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.Gateway;

namespace Backend.Tests.Gateway
{
    public class LoginRepoTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private IConnectionService _connectionService;
        private LoginRepo _loginRepo;

        public LoginRepoTests()
        {
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgreSqlContainer.GetConnectionString()
                })
                .Build();

            _connectionService = new ConnectionService(configuration);

            // Create table and seed test data
            await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await connection.OpenAsync();

            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS tblUserCredentials (
                    fldCredentialsID SERIAL PRIMARY KEY,
                    fldUsername VARCHAR(255) UNIQUE NOT NULL,
                    fldPassword VARCHAR(255) NOT NULL
                );";
            await createTableCmd.ExecuteNonQueryAsync();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO tblUserCredentials (fldUsername, fldPassword) VALUES
                ('john_doe', 'password123'),
                (' jane_doe ', ' pass123 '),   -- spaces to test trimming
                ('test_user', 'test_pass');";
            await insertCmd.ExecuteNonQueryAsync();

            _loginRepo = new LoginRepo(_connectionService);
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        [Fact]
        public void GetCredentials_ExistingUser_ReturnsUsernameAndPasswordTrimmed()
        {
            // Act
            var result = _loginRepo.getCredentials("john_doe");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("john_doe", result[0]);
            Assert.Equal("password123", result[1]);
        }

        [Fact]
        public void GetCredentials_ExistingUserWithSpaces_ReturnsTrimmedValues()
        {
            // Act
            var result = _loginRepo.getCredentials(" jane_doe "); // username with spaces

            // Assert: the query matches exactly, but the repo trims the returned values
            Assert.Equal(2, result.Count);
            Assert.Equal("jane_doe", result[0]);
            Assert.Equal("pass123", result[1]);
        }

        [Fact]
        public void GetCredentials_NonExistingUser_ReturnsEmptyList()
        {
            // Act
            var result = _loginRepo.getCredentials("nonexistent");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCredentials_NullUsername_ReturnsEmptyList()
        {
            // Act
            var result = _loginRepo.getCredentials(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCredentials_EmptyUsername_ReturnsEmptyList()
        {
            // Act
            var result = _loginRepo.getCredentials("");

            // Assert
            Assert.Empty(result);
        }
    }
}