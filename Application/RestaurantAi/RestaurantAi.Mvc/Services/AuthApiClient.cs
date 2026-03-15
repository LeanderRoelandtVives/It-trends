using System.Net.Http.Json;
using System.Text.Json;
using RestaurantAi.Dto.Request;
using Microsoft.Extensions.Logging;

namespace RestaurantAi.Mvc.Services;

public class AuthApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AuthApiClient> _log;

    public AuthApiClient(IHttpClientFactory httpClientFactory, ILogger<AuthApiClient> log)
    {
        _http = httpClientFactory.CreateClient("api");
        _log = log;
    }

    public async Task<string?> RegisterAsync(RegisterRequest req)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", req);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("Register request failed with status {Status}", res.StatusCode);
                return null;
            }

            var obj = await res.Content.ReadFromJsonAsync<JsonElement>();
            if (obj.ValueKind != JsonValueKind.Undefined && obj.TryGetProperty("token", out var tok)) return tok.GetString();
            return null;
        }
        catch (HttpRequestException ex)
        {
            _log.LogWarning(ex, "Register request to auth API failed");
            return null;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error in RegisterAsync");
            return null;
        }
    }

    public async Task<string?> LoginAsync(LoginRequest req)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", req);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("Login request failed with status {Status}", res.StatusCode);
                return null;
            }

            var obj = await res.Content.ReadFromJsonAsync<JsonElement>();
            if (obj.ValueKind != JsonValueKind.Undefined && obj.TryGetProperty("token", out var tok)) return tok.GetString();
            return null;
        }
        catch (HttpRequestException ex)
        {
            _log.LogWarning(ex, "Login request to auth API failed");
            return null;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error in LoginAsync");
            return null;
        }
    }
}
