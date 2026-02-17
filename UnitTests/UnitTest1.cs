using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitTests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    // private readonly WebApplicationFactory<Program> _factory;

    // public AuthControllerTests(WebApplicationFactory<Program> factory)
    // {
    //     _factory = factory;
    // }
    //
    // [Fact]
    // public async Task TrainerLogin_MissingFields_ReturnsBadRequest()
    // {
    //     using var client = _factory.CreateClient();
    //
    //     var response = await client.PostAsJsonAsync("/api/apiauth/trainer-login", new { Username = "", Password = "" });
    //
    //     Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    // }
    //
    // [Fact]
    // public async Task TrainerLogin_InvalidCredentials_ReturnsUnauthorized()
    // {
    //     using var client = _factory.CreateClient();
    //
    //     var response = await client.PostAsJsonAsync("/api/apiauth/trainer-login", new { Username = "someone", Password = "wrong" });
    //
    //     Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    // }
}
