using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frontend.Service;
using Moq;
using Moq.Protected;
using Xunit;

namespace UnitTests.Frontend_UnitTests.Services;

public class APIServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly APIService _apiService;

    public APIServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _apiService = new APIService(_httpClient);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";
        var expectedJson = "{\"username\":\"testuser\",\"password\":\"testpass\"}";
        
        SetupMockHttpResponse(HttpStatusCode.OK, "Success");

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.True(result);
        
        // Verify the request was made correctly
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://localhost:8081/api/APILogin/Login_CheckCredentials" &&
                VerifyRequestContent(req, expectedJson)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsFalse()
    {
        // Arrange
        var username = "wronguser";
        var password = "wrongpass";
        
        SetupMockHttpResponse(HttpStatusCode.Unauthorized, "Unauthorized");

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LoginAsync_ServerError_ReturnsFalse()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";
        
        SetupMockHttpResponse(HttpStatusCode.InternalServerError, "Internal Server Error");

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LoginAsync_HttpRequestException_ThrowsException()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _apiService.LoginAsync(username, password));
    }

    [Fact]
    public async Task LoginAsync_EmptyUsernameAndPassword_HandlesCorrectly()
    {
        // Arrange
        var username = "";
        var password = "";
        var expectedJson = "{\"username\":\"\",\"password\":\"\"}";
        
        SetupMockHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.False(result);
        
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://localhost:8081/api/APILogin/Login_CheckCredentials" &&
                VerifyRequestContent(req, expectedJson)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LoginAsync_NullUsernameAndPassword_HandlesCorrectly()
    {
        // Arrange
        string username = null;
        string password = null;
        var expectedJson = "{\"username\":null,\"password\":null}";
        
        SetupMockHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.False(result);
        
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://localhost:8081/api/APILogin/Login_CheckCredentials" &&
                VerifyRequestContent(req, expectedJson)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LoginAsync_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var username = "user@#$%";
        var password = "pass@#$%";
        var expectedJson = "{\"username\":\"user@#$%\",\"password\":\"pass@#$%\"}";
        
        SetupMockHttpResponse(HttpStatusCode.OK, "Success");

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.True(result);
        
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://localhost:8081/api/APILogin/Login_CheckCredentials" &&
                VerifyRequestContent(req, expectedJson)
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LoginAsync_JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";
        
        // Create the expected login data
        var loginData = new { username, password };
        var expectedJson = JsonSerializer.Serialize(loginData);
        
        HttpRequestMessage capturedRequest = null;
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((request, token) => 
            {
                capturedRequest = request;
            })
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => 
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Success")
                };
            });

        // Act
        var result = await _apiService.LoginAsync(username, password);

        // Assert
        Assert.True(result);
        Assert.NotNull(capturedRequest);
        
        var contentString = await capturedRequest.Content.ReadAsStringAsync();
        
        // Normalize JSON strings for comparison
        var expectedNormalized = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(expectedJson));
        var actualNormalized = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(contentString));
        
        Assert.Equal(expectedNormalized, actualNormalized);
        Assert.Equal("application/json", capturedRequest.Content.Headers.ContentType.MediaType);
        Assert.Equal("utf-8", capturedRequest.Content.Headers.ContentType.CharSet);
    }

    #region Helper Methods

    private void SetupMockHttpResponse(HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    private bool VerifyRequestContent(HttpRequestMessage request, string expectedJson)
    {
        if (request.Content == null)
            return false;

        var contentString = request.Content.ReadAsStringAsync().Result;
        
        try
        {
            var expectedNormalized = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(expectedJson));
            var actualNormalized = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(contentString));
            return expectedNormalized == actualNormalized;
        }
        catch
        {
            return contentString == expectedJson;
        }
    }

    #endregion
}