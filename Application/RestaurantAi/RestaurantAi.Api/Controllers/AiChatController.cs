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
                    ? "Hoi! ?? Ik ben je AI-assistent. Ik kan:\n\n??? Restaurants zoeken\n?? Vragen beantwoorden\n?? Dingen opzoeken\n\nVb: 'Restaurants in Amsterdam', 'Wat is de hoofdstad van Frankrijk?', 'Pizza dicht bij mij'"
                    : "Hi! ?? I'm your AI assistant. I can:\n\n??? Find restaurants\n?? Answer questions\n?? Look things up\n\nTry: 'Restaurants in Amsterdam', 'What is the capital of France?', 'Pizza near me'";
                return Ok(new ChatResponse(welcome, sessionId));
            }

            try
            {
                var message = request.Message.Trim();

                // Determine if this is a search for restaurants
                bool wantsRestaurantSearch = IsRestaurantQuery(message);

                if (wantsRestaurantSearch)
                {
                    // Handle restaurant search
                    string? extractedLocation = ExtractLocation(message);
                    var places = await QueryRestaurantsAsync(extractedLocation, request.Latitude, request.Longitude);
                    
                    if (places != null && places.Count > 0)
                    {
                        var summary = BuildPlacesSummary(places, request.Language);
                        return Ok(new ChatResponse(summary, sessionId));
                    }
                    
                    // If no restaurants found, still try AI response
                }

                // For general questions or when restaurant search fails, use Groq AI
                var aiResponse = await GetGroqResponse(message, request.Language);
                return Ok(new ChatResponse(aiResponse, sessionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat failure");
                var err = request.Language == "nl"
                    ? "Sorry, er is iets misgegaan. Probeer het opnieuw."
                    : "Sorry, something went wrong. Please try again.";
                return StatusCode(500, new ChatResponse(err, sessionId));
            }
        }

        private static bool IsRestaurantQuery(string message)
        {
            return message.Contains("restaurant", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("near me", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("nearby", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("pizza", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("food", StringComparison.OrdinalIgnoreCase)
                   || (message.Contains("eat", StringComparison.OrdinalIgnoreCase) && !message.Contains("wheat", StringComparison.OrdinalIgnoreCase))
                   || message.Contains("cuisine", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("dinner", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("lunch", StringComparison.OrdinalIgnoreCase)
                   || (message.Contains("in ", StringComparison.OrdinalIgnoreCase) && 
                       (message.Contains("find", StringComparison.OrdinalIgnoreCase) || 
                        message.Contains("restaurant", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("food", StringComparison.OrdinalIgnoreCase)));
        }

        private async Task<string> GetGroqResponse(string message, string language)
        {
            try
            {
                var apiKey = _config["Groq:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("Groq API key not configured");
                    return language == "nl" 
                        ? "AI-servic is niet geconfigureerd." 
                        : "AI service is not configured.";
                }

                var client = _httpClientFactory.CreateClient();
                
                var systemPrompt = language == "nl"
                    ? "Je bent een behulpzame AI-assistent die vragen beantwoordt, informatie opzoekt en helpt bij restaurantaanbevelingen. Antwoord in Nederlands."
                    : "You are a helpful AI assistant that answers questions, looks things up, and helps with restaurant recommendations. Answer in English.";

                var payload = new
                {
                    model = "mixtral-8x7b-32768",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = message }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                using var response = await client.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Groq API error: {Status} - {Response}", response.StatusCode, errorContent);
                    return language == "nl"
                        ? "Ik kon geen antwoord genereren. Probeer het opnieuw."
                        : "I couldn't generate a response. Please try again.";
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var choices = doc.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    var content = firstChoice.GetProperty("message").GetProperty("content").GetString();
                    return content ?? (language == "nl" ? "Geen antwoord beschikbaar." : "No response available.");
                }

                return language == "nl" ? "Geen antwoord beschikbaar." : "No response available.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq API call failed");
                return language == "nl"
                    ? "Er is een fout opgetreden bij het ophalen van het antwoord."
                    : "An error occurred while getting the response.";
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
                _logger.LogInformation("=== RESTAURANT SEARCH DEBUG ===");
                _logger.LogInformation("Location: {Location}, Lat: {Lat}, Lng: {Lng}", location ?? "null", lat, lng);
                
                string overpassQuery;
                string debugLocation = location ?? "none";

                if (lat.HasValue && lng.HasValue)
                {
                    _logger.LogInformation("Using GPS coordinates: {Lat}, {Lng}", lat, lng);
                    
                    // Try with larger radius first (for small cities)
                    var results = await TryRestaurantSearchWithRadius(lat.Value, lng.Value, 0.05, client); // ~5km
                    
                    if (results != null && results.Count > 0)
                    {
                        _logger.LogInformation("? Found {Count} restaurants with 5km radius", results.Count);
                        return results.Take(5).ToList();
                    }
                    
                    // If no results, try even larger radius
                    _logger.LogInformation("? No results with 5km radius, trying 10km radius...");
                    results = await TryRestaurantSearchWithRadius(lat.Value, lng.Value, 0.1, client); // ~10km
                    
                    if (results != null && results.Count > 0)
                    {
                        _logger.LogInformation("? Found {Count} restaurants with 10km radius", results.Count);
                        return results.Take(5).ToList();
                    }
                    
                    _logger.LogWarning("? No restaurants found with Overpass even with 10km radius");
                    _logger.LogInformation("Trying alternative search methods...");
                    
                    // Try with city name if we have coordinates (reverse geocode)
                    var cityName = await GetCityNameFromCoordinates(lat.Value, lng.Value, client);
                    if (!string.IsNullOrWhiteSpace(cityName))
                    {
                        _logger.LogInformation("Reverse geocoded to city: {City}", cityName);
                        results = await QueryRestaurantsAsync(cityName, null, null);
                        if (results != null && results.Count > 0)
                            return results;
                    }
                    
                    return new List<PlaceResult>();
                }
                else if (!string.IsNullOrWhiteSpace(location))
                {
                    _logger.LogInformation("Searching by location name: {Location}", location);
                    // First, try to get coordinates for the location name using Nominatim
                    var coords = await GetLocationCoordinatesAsync(location, client);
                    
                    if (coords != null)
                    {
                        _logger.LogInformation("? Found coordinates for {Location}: {Lat}, {Lng}", location, coords.Lat, coords.Lng);
                        
                        // Try with larger radius for location-based searches
                        var results = await TryRestaurantSearchWithRadius(coords.Lat, coords.Lng, 0.05, client); // ~5km
                        
                        if (results != null && results.Count > 0)
                        {
                            _logger.LogInformation("? Found {Count} restaurants for {Location}", results.Count, location);
                            return results.Take(5).ToList();
                        }
                        
                        // Try even larger radius
                        _logger.LogInformation("? No results with 5km, trying 10km radius for {Location}...", location);
                        results = await TryRestaurantSearchWithRadius(coords.Lat, coords.Lng, 0.1, client); // ~10km
                        
                        if (results != null && results.Count > 0)
                        {
                            _logger.LogInformation("? Found {Count} restaurants with 10km radius for {Location}", results.Count, location);
                            return results.Take(5).ToList();
                        }
                    }
                    else
                    {
                        _logger.LogWarning("? Could not geocode location: {Location}", location);
                    }
                    
                    // Fallback: search by name globally
                    _logger.LogInformation("Trying global search by restaurant name for: {Location}", location);
                    overpassQuery = $@"[out:json];
(
  node[amenity=restaurant][name~""{location}"",i];
  way[amenity=restaurant][name~""{location}"",i];
  relation[amenity=restaurant][name~""{location}"",i];
);
out center 20;";
                    
                    return await ExecuteOverpassQuery(overpassQuery, client, location);
                }
                else
                {
                    // Default to Amsterdam
                    _logger.LogInformation("Using default location: Amsterdam");
                    return await TryRestaurantSearchWithRadius(52.3676, 4.9041, 0.05, client);
                }
                
                return new List<PlaceResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overpass API query failed");
                return null;
            }
        }

        private async Task<string?> GetCityNameFromCoordinates(double lat, double lng, HttpClient client)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lng}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "RestaurantAI/1.0");
                
                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                
                var address = doc.RootElement.GetProperty("address");
                
                // Try different address levels
                if (address.TryGetProperty("city", out var city))
                    return city.GetString();
                if (address.TryGetProperty("town", out var town))
                    return town.GetString();
                if (address.TryGetProperty("village", out var village))
                    return village.GetString();
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reverse geocoding failed");
                return null;
            }
        }

        private async Task<List<PlaceResult>?> TryRestaurantSearchWithRadius(double lat, double lng, double delta, HttpClient client)
        {
            double south = lat - delta;
            double west = lng - delta;
            double north = lat + delta;
            double east = lng + delta;
            
            // IMPORTANT: Use InvariantCulture to ensure periods (.) not commas (,) in decimal numbers
            string overpassQuery = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                @"[out:json][bbox:{0},{1},{2},{3}];
(
  node[amenity=restaurant];
  way[amenity=restaurant];
  relation[amenity=restaurant];
);
out center 20;", south, west, north, east);

            _logger.LogInformation("Overpass bbox query: south={South}, west={West}, north={North}, east={East}", south, west, north, east);
            return await ExecuteOverpassQuery(overpassQuery, client, $"{lat},{lng}");
        }

        private async Task<List<PlaceResult>?> ExecuteOverpassQuery(string overpassQuery, HttpClient client, string location)
        {
            try
            {
                var url = "https://overpass-api.de/api/interpreter";
                using var content = new StringContent(overpassQuery, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                _logger.LogInformation("Executing Overpass query for {Location}", location);
                
                using var resp = await client.PostAsync(url, content);
                
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Overpass API returned {Status} for location {Location}", resp.StatusCode, location);
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Overpass error: {Response}", errorContent.Substring(0, Math.Min(200, errorContent.Length)));
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

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overpass query execution failed");
                return null;
            }
        }
        
        private async Task<LocationCoordinates?> GetLocationCoordinatesAsync(string location, HttpClient client)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(location)}&format=json&limit=1&addressdetails=0";
                
                // Nominatim requires a User-Agent header
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "RestaurantAI/1.0");
                
                _logger.LogInformation("Geocoding location: {Location} with URL: {Url}", location, url);
                
                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Nominatim geocoding failed for {Location} with status {Status}", location, response.StatusCode);
                    return null;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var results = doc.RootElement;
                _logger.LogInformation("Nominatim returned {Count} results for {Location}", results.GetArrayLength(), location);
                
                if (results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                {
                    var first = results[0];
                    var latStr = first.GetProperty("lat").GetString();
                    var lonStr = first.GetProperty("lon").GetString();
                    
                    _logger.LogInformation("Nominatim result for {Location}: lat={Lat}, lon={Lon}", location, latStr, lonStr);
                    
                    if (double.TryParse(latStr, out var lat) && double.TryParse(lonStr, out var lng))
                    {
                        _logger.LogInformation("Successfully geocoded {Location} to ({Lat}, {Lng})", location, lat, lng);
                        return new LocationCoordinates { Lat = lat, Lng = lng };
                    }
                }

                _logger.LogWarning("No results from Nominatim for {Location}", location);
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
