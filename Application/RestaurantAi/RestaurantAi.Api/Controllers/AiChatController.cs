using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantAi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AiChatController> _logger;

        public AiChatController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<AiChatController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public record ChatRequest(string Message, string SessionId, string Language, double? Latitude, double? Longitude);
        public record ChatResponse(string Reply, string SessionId);

        [HttpPost("message")]
        public async Task<IActionResult> Message([FromBody] ChatRequest request)
        {
            var sessionId = string.IsNullOrWhiteSpace(request.SessionId) ? Guid.NewGuid().ToString("N") : request.SessionId;

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                var welcome = request.Language == "nl"
                    ? "Hoi! ?? Ik ben je restaurant conciërge AI. Waar kan ik je mee helpen?\n\nVb: 'Restaurants in Amsterdam', 'Pizza dicht bij mij', 'Goedkope eetgelegenheden in Brussel'"
                    : "Hi! ?? I'm your restaurant concierge AI. How can I help?\n\nTry: 'Restaurants in Amsterdam', 'Pizza near me', 'Cheap food in London'\n\n(If 'near me' doesn't work, you may have blocked location. Check the ?? icon in your address bar to enable it.)";
                return Ok(new ChatResponse(welcome, sessionId));
            }

            try
            {
                var message = request.Message.Trim();

                // Determine if this is a search for restaurants
                bool wantsSearch = message.Contains("restaurant", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("near me", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("nearby", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("find", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("in ", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("pizza", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("food", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("eat", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("cuisine", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("dinner", StringComparison.OrdinalIgnoreCase)
                                   || message.Contains("lunch", StringComparison.OrdinalIgnoreCase);

                if (!wantsSearch)
                {
                    var fallback = request.Language == "nl"
                        ? $"Ik begreep: \"{message}\". Probeer bijvoorbeeld: \"Vind restaurants bij mij in de buurt\" of \"Italiaanse restaurants in Amsterdam\"."
                        : $"I understood: \"{message}\". Try: \"Find restaurants near me\" or \"Italian restaurants in Amsterdam\".";
                    return Ok(new ChatResponse(fallback, sessionId));
                }

                string? extractedLocation = ExtractLocation(message);

                var places = await QueryRestaurantsAsync(extractedLocation, request.Latitude, request.Longitude);
                if (places == null || places.Count == 0)
                {
                    var noRes = request.Language == "nl"
                        ? "Kon geen restaurants vinden voor die locatie.\n\nProbeer:\n• Een andere plaats noemen (bv. 'restaurants in Amsterdam')\n• De locatiegegevens in te schakelen (klik ?? in de adresbalk)\n• Een restauranttype te noemen (bv. 'Italiaanse restaurants')"
                        : "I couldn't find restaurants for that location.\n\nTry:\n• Mentioning a different city (e.g., 'restaurants in London')\n• Enabling location (click ?? in the address bar)\n• Specifying a cuisine type (e.g., 'Italian restaurants')";
                    return Ok(new ChatResponse(noRes, sessionId));
                }

                var summary = BuildPlacesSummary(places, request.Language);

                // Just return the summary without trying to call Groq
                // The places summary is already good enough
                return Ok(new ChatResponse(summary, sessionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiChat failure");
                var err = request.Language == "nl"
                    ? "Sorry, er is iets misgegaan. Probeer het opnieuw."
                    : "Sorry, something went wrong. Please try again.";
                return StatusCode(500, new ChatResponse(err, sessionId));
            }
        }

        private static string? ExtractLocation(string message)
        {
            var lower = message.ToLowerInvariant();
            
            // If it's a "near me" query, return null so geolocation is used
            if (lower.Contains("near me") || lower.Contains("nearby") || lower.Contains("close to me"))
            {
                return null;
            }
            
            // Extract location after keywords
            var patterns = new[] { " in ", " near ", " around ", " by " };
            foreach (var p in patterns)
            {
                var idx = lower.IndexOf(p, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    var loc = message.Substring(idx + p.Length).Trim();
                    // Remove punctuation at the end
                    var end = loc.IndexOfAny(new[] { '.', ',', ';', '?', '!' });
                    if (end >= 0) loc = loc.Substring(0, end).Trim();
                    if (!string.IsNullOrWhiteSpace(loc)) return loc;
                }
            }

            // Try to extract cuisine type + location pattern (e.g., "Italian restaurants in Amsterdam")
            var cuisinePatterns = new[] { "restaurants", "restaurant", "food", "pizzeria", "cafe" };
            foreach (var cuisine in cuisinePatterns)
            {
                var idx = lower.IndexOf(cuisine, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    var afterCuisine = message.Substring(idx + cuisine.Length).Trim();
                    // Look for " in " after the cuisine word
                    foreach (var p in new[] { " in ", " near ", " at " })
                    {
                        var locIdx = afterCuisine.ToLowerInvariant().IndexOf(p, StringComparison.Ordinal);
                        if (locIdx >= 0)
                        {
                            var loc = afterCuisine.Substring(locIdx + p.Length).Trim();
                            var end = loc.IndexOfAny(new[] { '.', ',', ';', '?', '!' });
                            if (end >= 0) loc = loc.Substring(0, end).Trim();
                            if (!string.IsNullOrWhiteSpace(loc)) return loc;
                        }
                    }
                }
            }
            
            return null;
        }

        private async Task<List<PlaceResult>?> QueryRestaurantsAsync(string? location, double? lat, double? lng)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            try
            {
                string overpassQuery;
                string debugLocation = location ?? "none";

                if (lat.HasValue && lng.HasValue)
                {
                    _logger.LogInformation("Searching restaurants near coordinates: {Lat}, {Lng}", lat, lng);
                    // Search near coordinates (approximately 3km radius using bbox)
                    double delta = 0.027; // roughly 3km
                    double latValue = lat.Value;
                    double lngValue = lng.Value;
                    overpassQuery = $@"[out:json];
(
  node[amenity=restaurant]({latValue - delta},{lngValue - delta},{latValue + delta},{lngValue + delta});
  way[amenity=restaurant]({latValue - delta},{lngValue - delta},{latValue + delta},{lngValue + delta});
  relation[amenity=restaurant]({latValue - delta},{lngValue - delta},{latValue + delta},{lngValue + delta});
);
out center 20;";
                }
                else if (!string.IsNullOrWhiteSpace(location))
                {
                    _logger.LogInformation("Searching restaurants by location: {Location}", location);
                    // First, try to get coordinates for the location name using Nominatim
                    var coords = await GetLocationCoordinatesAsync(location, client);
                    
                    if (coords != null)
                    {
                        _logger.LogInformation("Found coordinates for {Location}: {Lat}, {Lng}", location, coords.Lat, coords.Lng);
                        double delta = 0.027;
                        overpassQuery = $@"[out:json];
(
  node[amenity=restaurant]({coords.Lat - delta},{coords.Lng - delta},{coords.Lat + delta},{coords.Lng + delta});
  way[amenity=restaurant]({coords.Lat - delta},{coords.Lng - delta},{coords.Lat + delta},{coords.Lng + delta});
  relation[amenity=restaurant]({coords.Lat - delta},{coords.Lng - delta},{coords.Lat + delta},{coords.Lng + delta});
);
out center 20;";
                    }
                    else
                    {
                        _logger.LogWarning("Could not geocode location: {Location}", location);
                        // Fallback: search by name in wider area (worldwide)
                        overpassQuery = $@"[out:json];
(
  node[amenity=restaurant][name~""{location}"",i];
  way[amenity=restaurant][name~""{location}"",i];
  relation[amenity=restaurant][name~""{location}"",i];
);
out center 20;";
                    }
                }
                else
                {
                    // Default to Amsterdam
                    _logger.LogInformation("Using default location: Amsterdam");
                    overpassQuery = @"[out:json];
(
  node[amenity=restaurant](52.3602,-4.8952,52.4170,-4.7632);
  way[amenity=restaurant](52.3602,-4.8952,52.4170,-4.7632);
  relation[amenity=restaurant](52.3602,-4.8952,52.4170,-4.7632);
);
out center 20;";
                }

                var url = "https://overpass-api.de/api/interpreter";
                using var content = new StringContent(overpassQuery, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                _logger.LogDebug("Overpass query: {Query}", overpassQuery);
                using var resp = await client.PostAsync(url, content);
                
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Overpass API returned {Status} for location {Location}", resp.StatusCode, debugLocation);
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Overpass error response: {Response}", errorContent);
                    return null;
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var results = new List<PlaceResult>();

                if (doc.RootElement.TryGetProperty("elements", out var elements))
                {
                    _logger.LogInformation("Overpass returned {Count} elements", elements.GetArrayLength());
                    foreach (var el in elements.EnumerateArray())
                    {
                        string? name = null;
                        double lat_val = 0;
                        double lng_val = 0;

                        // Get name
                        if (el.TryGetProperty("tags", out var tags) && tags.TryGetProperty("name", out var nameEl))
                        {
                            name = nameEl.GetString();
                        }

                        // Get coordinates - prefer center, fallback to lat/lon
                        if (el.TryGetProperty("center", out var center))
                        {
                            if (center.TryGetProperty("lat", out var latEl))
                                lat_val = latEl.GetDouble();
                            if (center.TryGetProperty("lon", out var lonEl))
                                lng_val = lonEl.GetDouble();
                        }
                        else
                        {
                            if (el.TryGetProperty("lat", out var latEl))
                                lat_val = latEl.GetDouble();
                            if (el.TryGetProperty("lon", out var lonEl))
                                lng_val = lonEl.GetDouble();
                        }

                        if (!string.IsNullOrWhiteSpace(name) && (lat_val != 0 || lng_val != 0))
                        {
                            results.Add(new PlaceResult
                            {
                                Name = name,
                                Address = location ?? "Location unknown",
                                Rating = 0,
                                Reviews = 0,
                                PlaceId = $"{lat_val},{lng_val}"
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("No elements property in Overpass response");
                }

                _logger.LogInformation("Found {Count} restaurants for location {Location}", results.Count, debugLocation);
                return results.Take(5).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overpass API query failed");
                return null;
            }
        }

        private async Task<LocationCoordinates?> GetLocationCoordinatesAsync(string location, HttpClient client)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(location)}&format=json&limit=1";
                
                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Nominatim geocoding failed for {Location}", location);
                    return null;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var results = doc.RootElement;
                if (results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                {
                    var first = results[0];
                    if (double.TryParse(first.GetProperty("lat").GetString(), out var lat) &&
                        double.TryParse(first.GetProperty("lon").GetString(), out var lng))
                    {
                        return new LocationCoordinates { Lat = lat, Lng = lng };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Geocoding failed for location {Location}", location);
                return null;
            }
        }
        
        private static string BuildPlacesSummary(List<PlaceResult> places, string language)
        {
            var sb = new StringBuilder();
            if (language == "nl")
                sb.AppendLine("Ik heb de volgende restaurants gevonden:");
            else
                sb.AppendLine("Here are some restaurants I found:");

            int i = 1;
            foreach (var p in places)
            {
                sb.AppendLine($"{i}. {p.Name} • {p.Address}");
                sb.AppendLine($"   https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(p.Name)}");
                i++;
            }

            sb.AppendLine();
            if (language == "nl")
                sb.Append("Wil je dat ik meer details geef of een reservering maak?");
            else
                sb.Append("Would you like more details or to make a reservation?");
            return sb.ToString();
        }

        private class PlaceResult
        {
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public double Rating { get; set; }
            public int Reviews { get; set; }
            public string PlaceId { get; set; } = "";
        }

        private class LocationCoordinates
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }

    internal static class JsonExtensions
    {
        public static string? GetPropertyOrNull(this JsonElement element, string propName)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;
            if (element.TryGetProperty(propName, out var p))
            {
                return p.ValueKind switch
                {
                    JsonValueKind.String => p.GetString(),
                    JsonValueKind.Number => p.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => p.GetRawText()
                };
            }
            return null;
        }
    }
}
