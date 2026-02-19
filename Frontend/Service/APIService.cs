using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Service;

public class APIService : IAPIService
{
    
    private static readonly HttpClient _client = CreateHttpClient();
    private static readonly string _baseUrl = "http://localhost:8081";

    /// <summary>
    /// creates a HTTP client
    /// </summary>
    /// <returns></returns>
    private static HttpClient CreateHttpClient()
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
        Console.WriteLine("Login Data:" + loginData.username +  ":" + loginData.password);
        // Serialize the login data to JSON
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        Console.WriteLine("Raw Json:" + content);
        
        // Make the POST request with the JSON content
        var response = await _client.PostAsync($"{_baseUrl}/api/APILogin/Login_CheckCredentials", content);
        Console.WriteLine("Response:" + response);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Login successful");
            return true; // Login successful
        }

        return false; // Login failed
    }


    
}