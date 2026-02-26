using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using Backend.Application.Service;
using Backend.Application.Service.Interfaces;

namespace Backend.Tests.Application.Service
{
    public class DatabaseSeedingServiceTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private IConnectionService _connectionService;
        private IDatabaseSeedingService _seedingService;

        public DatabaseSeedingServiceTests()
        {
            // Spin up a disposable PostgreSQL container
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
            _seedingService = new DatabaseSeedingService(_connectionService);
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        [Fact]
        public async Task SeedTables_ShouldCreateAllTables()
        {
            // Act
            _seedingService.SeedTables();

            // Assert – verify each table exists
            await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await connection.OpenAsync();

            var expectedTables = new[]
            {
                "tblusercredentials",
                "tbluser",
                "tblworkout",
                "tbluserworkout",
                "tblexercise",
                "tblset",
                "tblworkoutset"
            };

            foreach (var table in expectedTables)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_name = @table
                    );";
                cmd.Parameters.AddWithValue("@table", table);
                var exists = (bool)await cmd.ExecuteScalarAsync();
                Assert.True(exists, $"Table '{table}' was not created.");
            }
        }

        [Fact]
        public async Task Seed_ShouldCreateTablesAndInsertTestData()
        {
            // Act
            _seedingService.Seed();

            // Assert
            await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await connection.OpenAsync();

            // Tables should exist (quick check)
            var tableCheckCmd = connection.CreateCommand();
            tableCheckCmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';";
            var tableCount = (long)await tableCheckCmd.ExecuteScalarAsync();
            Assert.Equal(7, tableCount); // 7 tables expected

            // Test data should exist (check for john_doe)
            var userCheckCmd = connection.CreateCommand();
            userCheckCmd.CommandText = @"
                SELECT COUNT(*) > 0 
                FROM tblUserCredentials 
                WHERE fldUsername = 'john_doe';";
            var userExists = (bool)await userCheckCmd.ExecuteScalarAsync();
            Assert.True(userExists, "Test user credentials were not inserted.");

            // Verify data in related tables (optional)
            var workoutCheckCmd = connection.CreateCommand();
            workoutCheckCmd.CommandText = "SELECT COUNT(*) FROM tblWorkout;";
            var workoutCount = (long)await workoutCheckCmd.ExecuteScalarAsync();
            Assert.Equal(1, workoutCount);
        }

        [Fact]
        public async Task SeedTestData_WhenDataAlreadyExists_ShouldNotInsert()
        {
            // Arrange – create tables first
            _seedingService.SeedTables();

            // Manually insert a user credentials row with the username that TestDataExists() checks for ('test_user')
            await using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            await connection.OpenAsync();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO tblUserCredentials (fldUsername, fldPassword)
                VALUES ('test_user', 'dummy')
                RETURNING fldCredentialsID;";
            var insertedId = await insertCmd.ExecuteScalarAsync();
            Assert.NotNull(insertedId);

            // Capture current row count in tblUserCredentials
            var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM tblUserCredentials;";
            var initialCount = (long)await countCmd.ExecuteScalarAsync();

            // Act – call SeedTestData (should see existing data and skip inserts)
            _seedingService.SeedTestData();

            // Assert – no new rows should be added
            var finalCount = (long)await countCmd.ExecuteScalarAsync();
            Assert.Equal(initialCount, finalCount);

            // Also verify that no rows were inserted into other tables (e.g., tblUser should be empty)
            var userTableCheck = connection.CreateCommand();
            userTableCheck.CommandText = "SELECT COUNT(*) FROM tblUser;";
            var userCount = (long)await userTableCheck.ExecuteScalarAsync();
            Assert.Equal(0, userCount);
        }
    }
}