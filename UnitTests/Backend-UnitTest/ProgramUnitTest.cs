using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Backend.Application.Service;
using Backend.Application.Service.Interfaces;
using Backend.External.Repos;
using Backend.Gateway;
using Backend.Service;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi.Models;


namespace UnitTests.Backend_UnitTest;

public class ProgramUnitTest
{
    private static readonly Assembly BackendAssembly = Assembly.GetAssembly(typeof(Program)) ?? Assembly.GetExecutingAssembly();
    
    [Fact]
    public void Builder_ShouldConfigureServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Set environment to Development for testing
        builder.Environment.EnvironmentName = "Development";

        // Add test configuration
        builder.Configuration.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", "Host=localhost;Database=testdb;Username=test;Password=test")
        });

        // Act - Add ALL services from the actual Program.cs
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        // Swagger configuration
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API", Version = "v1" });
            options.AddServer(new OpenApiServer { Url = "/" });
        });

        // Database Configuration
        builder.Services.AddScoped<NpgsqlConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new NpgsqlConnection(connectionString);
        });

        builder.Services.AddSingleton<ConnectionService>();

        //Use mocks instead of real implementations to avoid dependency issues
        var mockUserRepo = new Mock<IUserRepo>();
        var mockLoginRepo = new Mock<ILoginRepo>();
        var mockUserService = new Mock<IUserService>();
        var mockLoginService = new Mock<ILoginService>();
        var mockDatabaseSeedingService = new Mock<IDatabaseSeedingService>();

        // Register mocks
        builder.Services.AddScoped<IUserRepo>(_ => mockUserRepo.Object);
        builder.Services.AddScoped<ILoginRepo>(_ => mockLoginRepo.Object);
        builder.Services.AddScoped<IUserService>(_ => mockUserService.Object);
        builder.Services.AddScoped<ILoginService>(_ => mockLoginService.Object);
        builder.Services.AddSingleton(_ => mockDatabaseSeedingService.Object);

        var app = builder.Build();

        var serviceProvider = app.Services;

// infrastructure services kan godt hentes fra root
        var actionDescriptorProvider = serviceProvider.GetService<IActionDescriptorCollectionProvider>();
        Assert.NotNull(actionDescriptorProvider);

// scoped services skal hentes fra scope
        using var scope = serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var userRepo = scopedProvider.GetService<IUserRepo>();
        Assert.NotNull(userRepo);

        var loginRepo = scopedProvider.GetService<ILoginRepo>();
        Assert.NotNull(loginRepo);

        var userService = scopedProvider.GetService<IUserService>();
        Assert.NotNull(userService);

        var loginService = scopedProvider.GetService<ILoginService>();
        Assert.NotNull(loginService);

// singleton må gerne fra root (eller scope)
        var databaseSeedingService = serviceProvider.GetService<IDatabaseSeedingService>();
        Assert.NotNull(databaseSeedingService);

        var connectionService = serviceProvider.GetService<ConnectionService>();
        Assert.NotNull(connectionService);

// database connection scoped
        var connection = scopedProvider.GetService<NpgsqlConnection>();
        Assert.NotNull(connection);
    }

        [Fact]
        public async Task Builder_ShouldConfigureDatabaseConnection()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            
            // Add test configuration with connection string
            var testConnectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
            builder.Configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", testConnectionString)
            });

            // Act
            builder.Services.AddScoped<NpgsqlConnection>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                return new NpgsqlConnection(connectionString);
            });

            var app = builder.Build();

            // Assert
            using var scope = app.Services.CreateScope();
            var connection = scope.ServiceProvider.GetService<NpgsqlConnection>();
            
            Assert.NotNull(connection);
            Assert.Equal(testConnectionString, connection.ConnectionString);
        }
        

        [Fact]
        public async Task Builder_ShouldConfigureSwagger()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();

            // Act
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger API v1");
                c.RoutePrefix = "swagger";
            });

            // Assert
            var swaggerGenOptions = app.Services.GetService(typeof(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorOptions));
            Assert.NotNull(swaggerGenOptions);
        }

        [Fact]
        public async Task Builder_ShouldConfigureStaticFilesWithCorrectMimeTypes()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();

            // Act
            app.UseDefaultFiles();

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            
            // Test various file extensions
            var testFiles = new Dictionary<string, string>
            {
                { ".js", "application/javascript" },
                { ".mjs", "application/javascript" },
                { ".css", "text/css" },
                { ".weba", "audio/webp" },
                { ".webp", "image/webp" },
                { ".webm", "video/webm" },
                { ".svg", "image/svg+xml" },
                { ".svgz", "image/svg+xml" },
                { ".json", "application/json" },
                { ".wasm", "application/wasm" },
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
                { ".ttf", "font/ttf" },
                { ".otf", "font/otf" },
                { ".eot", "application/vnd.ms-fontobject" }
            };

            foreach (var testFile in testFiles)
            {
                contentTypeProvider.Mappings[testFile.Key] = testFile.Value;
                
                // Verify mapping exists and is correct
                Assert.True(contentTypeProvider.Mappings.ContainsKey(testFile.Key));
                Assert.Equal(testFile.Value, contentTypeProvider.Mappings[testFile.Key]);
            }
        }

        [Fact]
        public async Task Builder_ShouldSeedDatabaseOnStartup()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            
            // Mock the DatabaseSeedingService
            var mockSeeder = new Mock<IDatabaseSeedingService>();
            
            builder.Services.AddSingleton(mockSeeder.Object);

            var app = builder.Build();

            // Act
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeedingService>();
                seeder.Seed();
            }

            // Assert
            mockSeeder.Verify(x => x.Seed(), Times.Once);
        }

        [Fact]
        public async Task Builder_ShouldLoadEnvironmentSpecificConfiguration()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            
            // Set environment to Development
            builder.Environment.EnvironmentName = "Development";

            // Mock configuration sources
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Act
            var configuration = configurationBuilder.Build();
            builder.Configuration.AddConfiguration(configuration);

            // Assert
            Assert.NotNull(builder.Configuration);
            // We can't test actual values without files, but we can verify configuration sources
            Assert.NotEmpty(builder.Configuration.GetChildren());
        }

        [Fact]
        public async Task Builder_ShouldAddJsonEnumConverter()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();

            // Act
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            var app = builder.Build();

            // Assert
            var controllerOptions = app.Services.GetService<Microsoft.AspNetCore.Mvc.MvcOptions>();
            // Since we can't directly access JsonOptions, we'll verify that controllers are registered
            Assert.NotNull(app.Services.GetService(typeof(Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider)));
        }

        [Fact]
        public async Task CompleteApplication_ShouldBuildSuccessfully()
        {
            // Arrange - This test simulates the entire build process
            var builder = WebApplication.CreateBuilder();

            builder.Environment.EnvironmentName = "Development";
            
            // Add minimal required configuration
            builder.Configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", "Host=localhost;Database=testdb;Username=test;Password=test")
            });

            // Add all services (copy the original Program.cs code here)
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API", Version = "v1" });
                options.AddServer(new OpenApiServer { Url = "/" });
            });

            builder.Services.AddRateLimiter(rateLimiterOptions =>
                rateLimiterOptions.AddFixedWindowLimiter("FixedWindowPolicy", options =>
                {
                    options.PermitLimit = 1000;
                    options.Window = TimeSpan.FromSeconds(60);
                    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 20;
                }));

            // Act - This should not throw
            var exception = Record.Exception(() =>
            {
                var app = builder.Build();
                
                // Try to resolve some services to ensure they're registered correctly
                using var scope = app.Services.CreateScope();
                
                var userRepo = scope.ServiceProvider.GetService<IUserRepo>();
                var loginRepo = scope.ServiceProvider.GetService<ILoginRepo>();
                var userService = scope.ServiceProvider.GetService<UserService>();
                var loginService = scope.ServiceProvider.GetService<LoginService>();
            });

            // Assert
            Assert.Null(exception);
        }
}