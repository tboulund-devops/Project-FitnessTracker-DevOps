using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Service;

public class APIService : IAPIService
{
    private readonly HttpClient _client;
    private readonly string _baseUrl = "http://localhost:8081";

    public APIService() : this(CreateDefaultHttpClient())
    {
    }

    public APIService(HttpClient httpClient)
    {
        _client = httpClient;
    }

    /// <summary>
    /// creates a HTTP client
    /// </summary>
    /// <returns></returns>
    private static HttpClient CreateDefaultHttpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        return new HttpClient(handler);
    }
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        var loginData = new { username, password };
        // Serialize the login data to JSON
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Make the POST request with the JSON content
        var response = await _client.PostAsync($"{_baseUrl}/api/APILogin/Login_CheckCredentials", content);
        if (response.IsSuccessStatusCode)
        {
            return true; // Login successful
        }

        return false; // Login failed
    }
}