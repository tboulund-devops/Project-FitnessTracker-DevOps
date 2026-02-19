using System;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;
using Backend.Application.Service;
using Testcontainers.PostgreSql;

namespace UnitTests.Backend_UnitTest;

public class DatabaseSeedingTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private ConnectionService _connectionService;
    private DatabaseSeedingService _service;
    private IConfiguration _configuration;

    public DatabaseSeedingTests()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start the container
        await _postgreSqlContainer.StartAsync();
        
        // Create configuration with the container's connection string
        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:DefaultConnection", _postgreSqlContainer.GetConnectionString()}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Use the actual ConnectionService with test configuration
        _connectionService = new ConnectionService(_configuration);
        _service = new DatabaseSeedingService(_connectionService);
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public void Seed_CreatesTableAndInsertsTestUser_WhenDatabaseIsEmpty()
    {
        // Act
        _service.Seed();

        // Assert
        using var connection = _connectionService.GetConnection();
        connection.Open();

        // Check if table exists
        using var checkTableCmd = new NpgsqlCommand(@"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_name = 'tblusercredentials'
            );", connection);
        
        var tableExists = (bool)checkTableCmd.ExecuteScalar();
        Assert.True(tableExists);

        // Check if test user was inserted
        using var checkUserCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection);
        
        var userCount = (long)checkUserCmd.ExecuteScalar();
        Assert.Equal(1, userCount);

        // Verify password
        using var getUserCmd = new NpgsqlCommand(@"
            SELECT fldPassword FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection);
        
        var password = (string)getUserCmd.ExecuteScalar();
        Assert.Equal("test", password);
    }

    [Fact]
    public void Seed_DoesNotInsertDuplicateUser_WhenTestUserAlreadyExists()
    {
        // Arrange - Insert a user first using the connection service
        using (var connection = _connectionService.GetConnection())
        {
            connection.Open();
            
            // Create table
            using var createCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS tblUserCredentials (
                    fldUsername VARCHAR(100) NOT NULL,
                    fldPassword VARCHAR(100) NOT NULL
                );", connection);
            createCmd.ExecuteNonQuery();

            // Insert test user with different password
            using var insertCmd = new NpgsqlCommand(@"
                INSERT INTO tblUserCredentials (fldUsername, fldPassword)
                VALUES ('test', 'existing_password');
            ", connection);
            insertCmd.ExecuteNonQuery();
        }

        // Act
        _service.Seed();

        // Assert
        using var connection1 = _connectionService.GetConnection();
        connection1.Open();

        using var checkUserCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection1);
        
        var userCount = (long)checkUserCmd.ExecuteScalar();
        Assert.Equal(1, userCount); // Still only one user

        // Verify password wasn't overwritten
        using var getUserCmd = new NpgsqlCommand(@"
            SELECT fldPassword FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection1);
        
        var password = (string)getUserCmd.ExecuteScalar();
        Assert.Equal("existing_password", password);
    }

    [Fact]
    public void Seed_HandlesExistingTable_WithoutErrors()
    {
        // Arrange - Create table first using connection service
        using (var connection = _connectionService.GetConnection())
        {
            connection.Open();
            
            using var createCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS tblUserCredentials (
                    fldUsername VARCHAR(100) NOT NULL,
                    fldPassword VARCHAR(100) NOT NULL
                );", connection);
            createCmd.ExecuteNonQuery();
        }

        // Act
        _service.Seed();

        // Assert - Should not throw exception and should insert user
        using var connection1 = _connectionService.GetConnection();
        connection1.Open();

        using var checkUserCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection1);
        
        var userCount = (long)checkUserCmd.ExecuteScalar();
        Assert.Equal(1, userCount);
    }

    [Fact]
    public void Seed_MultipleCalls_DoesNotCreateDuplicates()
    {
        // Act - Call Seed multiple times
        _service.Seed();
        _service.Seed();
        _service.Seed();

        // Assert
        using var connection = _connectionService.GetConnection();
        connection.Open();

        using var checkUserCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection);
        
        var userCount = (long)checkUserCmd.ExecuteScalar();
        Assert.Equal(1, userCount); // Still only one user
    }

    [Fact]
    public void Seed_WhenConnectionStringIsInvalid_ThrowsException()
    {
        // Arrange
        var invalidSettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:DefaultConnection", "Host=invalidhost;Port=9999;Database=testdb;Username=testuser;Password=testpassword"}
        };

        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(invalidSettings)
            .Build();

        var invalidConnectionService = new ConnectionService(invalidConfig);
        var service = new DatabaseSeedingService(invalidConnectionService);

        // Act & Assert
        Assert.Throws<NpgsqlException>(() => service.Seed());
    }

    [Fact]
    public void Seed_WhenDatabaseIsReadOnly_ThrowsException()
    {
        // Arrange - Create a read-only user
        using (var connection = _connectionService.GetConnection())
        {
            connection.Open();
            
            // Create a read-only user
            using var createUserCmd = new NpgsqlCommand(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'readonly_user') THEN
                        CREATE USER readonly_user WITH PASSWORD 'readonly_password';
                    END IF;
                END
                $$;
                
                REVOKE CREATE ON SCHEMA public FROM PUBLIC;
                REVOKE CREATE ON SCHEMA public FROM readonly_user;
                GRANT CONNECT ON DATABASE testdb TO readonly_user;
                GRANT USAGE ON SCHEMA public TO readonly_user;
                GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;
            ", connection);
            createUserCmd.ExecuteNonQuery();
        }

        // Create connection service with read-only user
        var readOnlySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:DefaultConnection", 
                $"Host=localhost;Port={_postgreSqlContainer.GetMappedPublicPort(5432)};Database=testdb;Username=readonly_user;Password=readonly_password"}
        };

        var readOnlyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(readOnlySettings)
            .Build();

        var readOnlyConnectionService = new ConnectionService(readOnlyConfig);
        var service = new DatabaseSeedingService(readOnlyConnectionService);

        // Act & Assert
        var exception = Assert.Throws<PostgresException>(() => service.Seed());
        Assert.Contains("permission denied", exception.Message.ToLower());
    }

    [Fact]
    public void Seed_WhenDatabaseDoesNotExist_ThrowsException()
    {
        // Arrange
        var invalidSettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:DefaultConnection", 
                $"Host=localhost;Port={_postgreSqlContainer.GetMappedPublicPort(5432)};Database=non_existent_db;Username=testuser;Password=testpassword"}
        };

        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(invalidSettings)
            .Build();

        var invalidConnectionService = new ConnectionService(invalidConfig);
        var service = new DatabaseSeedingService(invalidConnectionService);

        // Act & Assert
        Assert.Throws<PostgresException>(() => service.Seed());
    }

    [Fact]
    public void Seed_WhenTableIsCorrupted_ThrowsException()
    {
        // Arrange - Create a table with wrong schema (different columns)
        using (var connection = _connectionService.GetConnection())
        {
            connection.Open();
            
            // Drop table if exists
            using var dropCmd = new NpgsqlCommand(@"
                DROP TABLE IF EXISTS tblUserCredentials;
            ", connection);
            dropCmd.ExecuteNonQuery();

            // Create table with wrong schema (missing password column)
            using var createWrongTableCmd = new NpgsqlCommand(@"
                CREATE TABLE tblUserCredentials (
                    fldUsername VARCHAR(100) NOT NULL,
                    fldWrongColumn VARCHAR(100) NOT NULL
                );
            ", connection);
            createWrongTableCmd.ExecuteNonQuery();
        }

        // Act & Assert - The INSERT will fail because of missing column
        var exception = Assert.Throws<PostgresException>(() => _service.Seed());
        Assert.Contains("column", exception.Message.ToLower());
    }

    [Fact]
    public void Seed_ExecutesSuccessfully_WithCleanDatabase()
    {
        // This test ensures no exceptions are thrown with a clean database
        
        // Act
        var exception = Record.Exception(() => _service.Seed());
        
        // Assert
        Assert.Null(exception);
        
        // Verify the data was inserted correctly
        using var connection = _connectionService.GetConnection();
        connection.Open();

        using var checkUserCmd = new NpgsqlCommand(@"
            SELECT fldUsername, fldPassword FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection);
        
        using var reader = checkUserCmd.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal("test", reader.GetString(0));
        Assert.Equal("test", reader.GetString(1));
    }

    [Fact]
    public void Seed_WhenTableHasDifferentCase_StillWorks()
    {
        // Arrange - Create table with different case
        using (var connection = _connectionService.GetConnection())
        {
            connection.Open();
            
            // PostgreSQL folds unquoted identifiers to lowercase
            using var createCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS TBLUSERCREDENTIALS (
                    FLDUSERNAME VARCHAR(100) NOT NULL,
            FLDPASSWORD VARCHAR(100) NOT NULL
                );
            ", connection);
            createCmd.ExecuteNonQuery();
        }

        // Act
        _service.Seed();

        // Assert
        using var connection1 = _connectionService.GetConnection();
        connection1.Open();

        using var checkUserCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM tblUserCredentials WHERE fldUsername = 'test';
        ", connection1);
        
        var userCount = (long)checkUserCmd.ExecuteScalar();
        Assert.Equal(1, userCount);
    }
}