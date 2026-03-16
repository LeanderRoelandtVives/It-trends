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
                var body = await res.Content.ReadAsStringAsync();
                _log.LogWarning("Register request failed with status {Status}. Response: {Body}", res.StatusCode, body);
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
                var body = await res.Content.ReadAsStringAsync();
                _log.LogWarning("Login request failed with status {Status}. Response: {Body}", res.StatusCode, body);
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

    // Reservations API wrappers
    public async Task<List<Reservation>?> GetReservationsAsync()
    {
        try
        {
            _log.LogInformation("Calling GET {Url}", _http.BaseAddress + "api/reservations");
            var res = await _http.GetAsync("api/reservations");
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _log.LogWarning("GetReservations failed with status {Status}. Response: {Body}", res.StatusCode, body);
                return null;
            }
            return await res.Content.ReadFromJsonAsync<List<Reservation>>();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "GetReservations failed");
            return null;
        }
    }

    public async Task<Reservation?> CreateReservationAsync(Reservation r)
    {
        try
        {
            var url = "api/reservations";
            var payload = JsonSerializer.Serialize(r);
            _log.LogInformation("Creating reservation. POST {Url} Payload: {Payload}", _http.BaseAddress + url, payload);

            var res = await _http.PostAsJsonAsync(url, r);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _log.LogWarning("CreateReservation failed with status {Status}. Response: {Body}", res.StatusCode, body);
                return null;
            }
            var created = await res.Content.ReadFromJsonAsync<Reservation>();
            _log.LogInformation("CreateReservation succeeded. Created id: {Id}", created?.Id);
            return created;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "CreateReservation failed");
            return null;
        }
    }

    public class UpdateResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Body { get; set; }
    }

    public async Task<UpdateResult> UpdateReservationAsync(int id, Reservation r)
    {
        try
        {
            var url = $"api/reservations/{id}";
            _log.LogInformation("Updating reservation. PUT {Url} Payload: {Payload}", _http.BaseAddress + url, JsonSerializer.Serialize(r));
            var res = await _http.PutAsJsonAsync(url, r);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("UpdateReservation failed with status {Status}. Response: {Body}", res.StatusCode, body);
            }
            else
            {
                _log.LogInformation("UpdateReservation succeeded for id {Id}", id);
            }
            return new UpdateResult { Success = res.IsSuccessStatusCode, StatusCode = (int)res.StatusCode, Body = body };
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "UpdateReservation failed");
            return new UpdateResult { Success = false, StatusCode = 0, Body = ex.Message };
        }
    }

    public async Task<bool> DeleteReservationAsync(int id)
    {
        try
        {
            var url = $"api/reservations/{id}";
            _log.LogInformation("Deleting reservation. DELETE {Url}", _http.BaseAddress + url);
            var res = await _http.DeleteAsync(url);
            if (!res.IsSuccessStatusCode)
                _log.LogWarning("DeleteReservation failed with status {Status}", res.StatusCode);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "DeleteReservation failed");
            return false;
        }
    }

    public async Task<Reservation?> GetReservationAsync(int id)
    {
        try
        {
            var url = $"api/reservations/{id}";
            _log.LogInformation("Calling GET {Url}", _http.BaseAddress + url);
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _log.LogWarning("GetReservation failed with status {Status}. Response: {Body}", res.StatusCode, body);
                return null;
            }
            return await res.Content.ReadFromJsonAsync<Reservation>();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "GetReservation failed");
            return null;
        }
    }
}

// Local lightweight Reservation DTO for MVC client
public class Reservation
{
    public int Id { get; set; }
    public string OwnerId { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; }
    public int PartySize { get; set; }
    public string? SpecialRequests { get; set; }
}
