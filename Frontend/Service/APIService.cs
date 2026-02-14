using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace FitnessTracker.UI.Service;

public class APIService : IAPIService
{
    
    private static readonly HttpClient _client = CreateHttpClient();
    //private static readonly string _baseUrl = 
    private static string _token = string.Empty;

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
        string json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");


        // var response = await _client.PostAsync($"{_baseUrl}APIAuth/admin-login", content);
        // if (response.IsSuccessStatusCode)
        // {
        //     var responseString = await response.Content.ReadAsStringAsync();
        //     UserDTO? adminDTO = JsonSerializer.Deserialize<UserDTO>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // }

        return false;

    }

    
}