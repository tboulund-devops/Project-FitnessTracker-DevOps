using Backend.Application.Service.Interfaces;

namespace UnitTests.Backend_UnitTest;

using System.Net;
using Backend.Application.Service;
using Backend.Service;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove real DatabaseSeedingService
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DatabaseSeedingService));

                if (descriptor != null)
                    services.Remove(descriptor);

                // Mock seeder so Seed() is called without DB dependency
                var mockSeeder = new Mock<IDatabaseSeedingService>();
                mockSeeder.Setup(s => s.Seed());

                services.AddSingleton(mockSeeder.Object);
            });
        });
    }

    [Fact]
    public async Task App_Starts_And_Swagger_Works()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StaticFiles_Are_Configured()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        // Even if index.html doesn't exist,
        // middleware pipeline is executed
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public void Services_Are_Registered()
    {
        using var scope = _factory.Services.CreateScope();

        var userService = scope.ServiceProvider.GetService<IUserService>();
        var loginService = scope.ServiceProvider.GetService<ILoginService>();
        var connectionService = scope.ServiceProvider.GetService<IConnectionService>();
        var seeder = scope.ServiceProvider.GetService<IDatabaseSeedingService>();

        Assert.NotNull(userService);
        Assert.NotNull(loginService);
        Assert.NotNull(connectionService);
        Assert.NotNull(seeder);
    }
}