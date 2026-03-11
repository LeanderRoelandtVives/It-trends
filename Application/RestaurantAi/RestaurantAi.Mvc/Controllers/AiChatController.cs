using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Services;
using OpenAI.Chat;

namespace RestaurantAi.Mvc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiChatController : ControllerBase
    {
        private readonly AIService _aiService;

        public AiChatController(AIService aiService)
        {
            _aiService = aiService;
        }

        public class ChatRequest { public string Message { get; set; } public string SessionId { get; set; } public string Language { get; set; } }
        public class ChatResponse { public string Reply { get; set; } public string SessionId { get; set; } public object Data { get; set; } }

        enum ConversationStep { Start, AwaitingCountry, AwaitingCity, ShowingOptions, AwaitingCuisine, AwaitingConfirmation }

        class ConversationState
        {
            public ConversationStep Step { get; set; } = ConversationStep.Start;
            public string Language { get; set; } = "en";
            public string Country { get; set; }
            public string City { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public List<Place> Places { get; set; } = new List<Place>();
            public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();
        }

        class Place
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Cuisine { get; set; }
        }

        private static readonly ConcurrentDictionary<string, ConversationState> Sessions = new ConcurrentDictionary<string, ConversationState>();
        private static readonly HttpClient Http = new HttpClient();

        static AiChatController()
        {
            // Nominatim requires a descriptive User-Agent and Referer
            try 
            { 
                Http.DefaultRequestHeaders.UserAgent.ParseAdd("RestaurantAi/1.0 (ASP.NET Core Application)"); 
                Http.DefaultRequestHeaders.Add("Referer", "http://localhost");
            } 
            catch { }
        }

        [HttpPost("message")]
        public async Task<IActionResult> PostMessage([FromBody] ChatRequest req)
        {
            var message = (req?.Message ?? string.Empty).Trim();
            var sessionId = string.IsNullOrEmpty(req?.SessionId) ? Guid.NewGuid().ToString("N") : req.SessionId;
            var lang = string.IsNullOrEmpty(req?.Language) ? "en" : req.Language;
            var state = Sessions.GetOrAdd(sessionId, _ => new ConversationState { Step = ConversationStep.Start, Language = lang });
            
            // Update language if changed
            state.Language = lang;

            // Add user message to chat history
            if (!string.IsNullOrWhiteSpace(message))
            {
                state.ChatHistory.Add(ChatMessage.CreateUserMessage(message));
            }

            // Start conversation with AI
            if (state.Step == ConversationStep.Start)
            {
                state.Step = ConversationStep.AwaitingCountry;
                
                var aiPrompt = lang == "nl" 
                    ? "De gebruiker start een gesprek. Verwelkom hen vriendelijk en vraag in welk land ze willen dineren. Wees enthousiast en behulpzaam."
                    : "The user is starting a conversation. Welcome them warmly and ask which country they'd like to dine in. Be enthusiastic and helpful.";
                
                var aiResponse = await _aiService.GetResponseAsync(aiPrompt, state.ChatHistory);
                state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(aiResponse));
                
                return Ok(new ChatResponse 
                { 
                    Reply = aiResponse, 
                    SessionId = sessionId 
                });
            }

            // Step 1: Get Country with AI
            if (state.Step == ConversationStep.AwaitingCountry)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    var emptyPrompt = lang == "nl"
                        ? "De gebruiker heeft geen land opgegeven. Vraag vriendelijk opnieuw."
                        : "The user didn't provide a country. Politely ask again.";
                    
                    var aiResponse = await _aiService.GetResponseAsync(emptyPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(aiResponse));
                    
                    return Ok(new ChatResponse { Reply = aiResponse, SessionId = sessionId });
                }

                state.Country = message;
                state.Step = ConversationStep.AwaitingCity;
                
                var cityPrompt = lang == "nl"
                    ? $"De gebruiker wil dineren in {message}. Bevestig dit enthousiast en vraag nu naar de stad of het gebied waar ze willen zoeken. Geef voorbeelden van bekende steden in dat land."
                    : $"The user wants to dine in {message}. Acknowledge this enthusiastically and now ask for the city or area they want to search. Give examples of well-known cities in that country.";
                
                var cityResponse = await _aiService.GetResponseAsync(cityPrompt, state.ChatHistory);
                state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(cityResponse));
                
                return Ok(new ChatResponse 
                { 
                    Reply = cityResponse, 
                    SessionId = sessionId 
                });
            }

            // Step 2: Get City and Search Restaurants with AI
            if (state.Step == ConversationStep.AwaitingCity)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    var emptyPrompt = lang == "nl"
                        ? "De gebruiker heeft geen stad opgegeven. Vraag vriendelijk opnieuw."
                        : "The user didn't provide a city. Politely ask again.";
                    
                    var aiResponse = await _aiService.GetResponseAsync(emptyPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(aiResponse));
                    
                    return Ok(new ChatResponse { Reply = aiResponse, SessionId = sessionId });
                }

                // Allow user to change country
                if (message.ToLowerInvariant().Contains("change country") || 
                    message.ToLowerInvariant().Contains("verander land") ||
                    message.ToLowerInvariant().Contains("ander land"))
                {
                    state.Step = ConversationStep.AwaitingCountry;
                    state.Country = null;
                    state.City = null;
                    
                    var resetPrompt = lang == "nl"
                        ? "De gebruiker wil een ander land kiezen. Bevestig dit vriendelijk en vraag opnieuw naar het land."
                        : "The user wants to change the country. Acknowledge this kindly and ask for the country again.";
                    
                    var resetResponse = await _aiService.GetResponseAsync(resetPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(resetResponse));
                    
                    return Ok(new ChatResponse { Reply = resetResponse, SessionId = sessionId });
                }

                state.City = message;

                // Geocode using multiple services with fallback
                try
                {
                    var searchQuery = $"{message}, {state.Country}";
                    double? lat = null;
                    double? lon = null;
                    string displayName = searchQuery;
                    
                    // Try Nominatim first
                    try
                    {
                        var q = Uri.EscapeDataString(searchQuery);
                        var nomUrl = $"https://nominatim.openstreetmap.org/search?q={q}&format=json&limit=1&addressdetails=1";
                        
                        await Task.Delay(1000); // Rate limiting
                        
                        using var request = new HttpRequestMessage(HttpMethod.Get, nomUrl);
                        request.Headers.Add("Accept-Language", "en");
                        
                        var response = await Http.SendAsync(request);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var nom = await response.Content.ReadAsStringAsync();
                            var nomResults = JsonSerializer.Deserialize<List<JsonElement>>(nom);
                            
                            if (nomResults != null && nomResults.Count > 0)
                            {
                                var first = nomResults[0];
                                if (first.TryGetProperty("lat", out var latEl) && first.TryGetProperty("lon", out var lonEl))
                                {
                                    lat = double.Parse(latEl.GetString() ?? "0");
                                    lon = double.Parse(lonEl.GetString() ?? "0");
                                    displayName = first.TryGetProperty("display_name", out var dispEl) 
                                        ? dispEl.GetString() 
                                        : searchQuery;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Nominatim failed, will try fallback
                    }
                    
                    // Fallback: Try photon.komoot.io
                    if (!lat.HasValue || !lon.HasValue)
                    {
                        try
                        {
                            var q = Uri.EscapeDataString(searchQuery);
                            var photonUrl = $"https://photon.komoot.io/api/?q={q}&limit=1&lang=en";
                            
                            var photonResponse = await Http.GetAsync(photonUrl);
                            if (photonResponse.IsSuccessStatusCode)
                            {
                                var photonResp = await photonResponse.Content.ReadAsStringAsync();
                                using var photonDoc = JsonDocument.Parse(photonResp);
                                
                                if (photonDoc.RootElement.TryGetProperty("features", out var features))
                                {
                                    var featArray = features.EnumerateArray().ToList();
                                    if (featArray.Count > 0)
                                    {
                                        var first = featArray[0];
                                        if (first.TryGetProperty("geometry", out var geom) && 
                                            geom.TryGetProperty("coordinates", out var coords))
                                        {
                                            var coordArray = coords.EnumerateArray().ToList();
                                            if (coordArray.Count >= 2)
                                            {
                                                lon = coordArray[0].GetDouble();
                                                lat = coordArray[1].GetDouble();
                                                
                                                if (first.TryGetProperty("properties", out var props))
                                                {
                                                    if (props.TryGetProperty("name", out var nameEl))
                                                        displayName = nameEl.GetString() ?? searchQuery;
                                                    else if (props.TryGetProperty("city", out var cityEl))
                                                        displayName = cityEl.GetString() ?? searchQuery;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Photon failed, try third service
                        }
                    }
                    
                    // Second fallback: Try geocode.xyz
                    if (!lat.HasValue || !lon.HasValue)
                    {
                        try
                        {
                            await Task.Delay(500);
                            
                            var q = Uri.EscapeDataString(searchQuery);
                            var geocodeUrl = $"https://geocode.xyz/{q}?json=1";
                            
                            var geocodeResponse = await Http.GetAsync(geocodeUrl);
                            if (geocodeResponse.IsSuccessStatusCode)
                            {
                                var geocodeResp = await geocodeResponse.Content.ReadAsStringAsync();
                                using var geocodeDoc = JsonDocument.Parse(geocodeResp);
                                
                                if (geocodeDoc.RootElement.TryGetProperty("latt", out var lattEl) && 
                                    geocodeDoc.RootElement.TryGetProperty("longt", out var longtEl))
                                {
                                    if (double.TryParse(lattEl.GetString(), out var parsedLat) &&
                                        double.TryParse(longtEl.GetString(), out var parsedLon))
                                    {
                                        lat = parsedLat;
                                        lon = parsedLon;
                                        
                                        if (geocodeDoc.RootElement.TryGetProperty("standard", out var standardEl) &&
                                            standardEl.TryGetProperty("city", out var cityNameEl))
                                        {
                                            displayName = cityNameEl.GetString() ?? searchQuery;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // All services failed
                        }
                    }
                    
                    if (!lat.HasValue || !lon.HasValue)
                    {
                        state.Step = ConversationStep.AwaitingCity;
                        
                        var notFoundPrompt = lang == "nl"
                            ? $"De locatie '{message}' kon niet gevonden worden in {state.Country}. Geef de gebruiker vriendelijke suggesties voor bekende steden in dat land en bied aan om een ander land te kiezen."
                            : $"The location '{message}' couldn't be found in {state.Country}. Give the user friendly suggestions for well-known cities in that country and offer to change the country.";
                        
                        var notFoundResponse = await _aiService.GetResponseAsync(notFoundPrompt, state.ChatHistory);
                        state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(notFoundResponse));
                        
                        return Ok(new ChatResponse { Reply = notFoundResponse, SessionId = sessionId });
                    }

                    state.Latitude = lat.Value;
                    state.Longitude = lon.Value;

                    // Query Overpass for nearby restaurants
                    var overpassQuery = $"[out:json][timeout:25];(node[amenity=restaurant](around:5000,{lat},{lon});way[amenity=restaurant](around:5000,{lat},{lon}););out center tags;";
                    var overpassUrl = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(overpassQuery);
                    
                    var overpassResp = await Http.GetStringAsync(overpassUrl);
                    using var doc = JsonDocument.Parse(overpassResp);
                    var places = new List<Place>();
                    
                    if (doc.RootElement.TryGetProperty("elements", out var elements))
                    {
                        foreach (var el in elements.EnumerateArray())
                        {
                            string name = null;
                            string address = null;
                            string cuisine = null;
                            
                            if (el.TryGetProperty("tags", out var tags))
                            {
                                if (tags.TryGetProperty("name", out var nameEl)) name = nameEl.GetString();
                                if (tags.TryGetProperty("addr:full", out var addrEl)) address = addrEl.GetString();
                                if (string.IsNullOrEmpty(address) && tags.TryGetProperty("addr:street", out var streetEl))
                                {
                                    var street = streetEl.GetString();
                                    if (tags.TryGetProperty("addr:housenumber", out var houseEl))
                                        address = $"{houseEl.GetString()} {street}";
                                    else
                                        address = street;
                                }
                                if (tags.TryGetProperty("cuisine", out var cEl)) cuisine = cEl.GetString();
                            }

                            if (string.IsNullOrWhiteSpace(name)) continue;

                            places.Add(new Place { Name = name, Address = address ?? "", Cuisine = cuisine ?? "Various" });
                        }
                    }

                    // Deduplicate and keep top 10
                    state.Places = places.GroupBy(p => p.Name).Select(g => g.First()).Take(10).ToList();
                    
                    if (state.Places.Count == 0)
                    {
                        state.Step = ConversationStep.AwaitingCity;
                        
                        var noRestaurantsPrompt = lang == "nl"
                            ? $"Er zijn geen restaurants gevonden binnen 5km van {displayName}. Stel vriendelijk andere locaties voor of bied aan om in een ander land te zoeken."
                            : $"No restaurants found within 5km of {displayName}. Kindly suggest trying other locations or offer to search in a different country.";
                        
                        var noRestaurantsResponse = await _aiService.GetResponseAsync(noRestaurantsPrompt, state.ChatHistory);
                        state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(noRestaurantsResponse));
                        
                        return Ok(new ChatResponse { Reply = noRestaurantsResponse, SessionId = sessionId });
                    }

                    state.Step = ConversationStep.AwaitingCuisine;

                    // Use AI to present restaurants naturally
                    var restaurantData = state.Places.Select(p => new { p.Name, p.Cuisine, p.Address }).ToList();
                    var restaurantJson = JsonSerializer.Serialize(restaurantData);
                    
                    var resultsPrompt = lang == "nl"
                        ? $"We hebben {state.Places.Count} restaurants gevonden in {displayName}. Hier is de data: {restaurantJson}. Presenteer deze restaurants op een vriendelijke, enthousiaste manier. Nummer ze van 1-{state.Places.Count}. Vermeld de keuken als beschikbaar. Leg uit dat ze kunnen filteren op keuken, een nummer kunnen kiezen voor details, of 'toon alles' kunnen typen."
                        : $"We found {state.Places.Count} restaurants in {displayName}. Here's the data: {restaurantJson}. Present these restaurants in a friendly, enthusiastic way. Number them 1-{state.Places.Count}. Mention cuisine when available. Explain they can filter by cuisine, choose a number for details, or type 'show all'.";
                    
                    var resultsResponse = await _aiService.GetRestaurantSearchResponseAsync(
                        resultsPrompt, 
                        state.ChatHistory,
                        displayName,
                        restaurantData.Cast<object>().ToList()
                    );
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(resultsResponse));

                    return Ok(new ChatResponse { Reply = resultsResponse, SessionId = sessionId, Data = state.Places.Select(p => new { p.Name, p.Cuisine }).ToList() });
                }
                catch (HttpRequestException ex)
                {
                    state.Step = ConversationStep.AwaitingCity;
                    
                    var errorPrompt = lang == "nl"
                        ? $"Er was een verbindingsprobleem met de locatieservice: {ex.Message}. Leg dit vriendelijk uit en vraag of ze het opnieuw willen proberen."
                        : $"There was a connection problem with the location service: {ex.Message}. Explain this kindly and ask if they'd like to try again.";
                    
                    var errorResponse = await _aiService.GetResponseAsync(errorPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(errorResponse));
                    
                    return Ok(new ChatResponse { Reply = errorResponse, SessionId = sessionId });
                }
                catch (JsonException)
                {
                    state.Step = ConversationStep.AwaitingCity;
                    
                    var errorPrompt = lang == "nl"
                        ? $"Er was een probleem bij het vinden van die locatie. Vraag vriendelijk of ze een bekende stad in {state.Country} kunnen proberen."
                        : $"There was a problem finding that location. Kindly ask if they can try a well-known city in {state.Country}.";
                    
                    var errorResponse = await _aiService.GetResponseAsync(errorPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(errorResponse));
                    
                    return Ok(new ChatResponse { Reply = errorResponse, SessionId = sessionId });
                }
                catch (Exception ex)
                {
                    state.Step = ConversationStep.AwaitingCity;
                    
                    var errorPrompt = lang == "nl"
                        ? $"Er ging iets mis tijdens het zoeken. Vraag vriendelijk of ze een bekende stad in {state.Country} kunnen proberen of een ander land willen kiezen."
                        : $"Something went wrong during the search. Kindly ask if they can try a well-known city in {state.Country} or change the country.";
                    
                    var errorResponse = await _aiService.GetResponseAsync(errorPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(errorResponse));
                    
                    return Ok(new ChatResponse { Reply = errorResponse, SessionId = sessionId });
                }
            }

            // Step 3: Filter by cuisine or select restaurant with AI
            if (state.Step == ConversationStep.AwaitingCuisine)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    var emptyPrompt = lang == "nl"
                        ? "De gebruiker heeft geen keuze gemaakt. Vraag vriendelijk of ze willen filteren op keuken of een restaurantnummer willen selecteren."
                        : "The user didn't make a choice. Kindly ask if they'd like to filter by cuisine or select a restaurant number.";
                    
                    var emptyResponse = await _aiService.GetResponseAsync(emptyPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(emptyResponse));
                    
                    return Ok(new ChatResponse { Reply = emptyResponse, SessionId = sessionId });
                }

                // Check if user wants to see all
                if (message.ToLowerInvariant().Contains("show all") || message.ToLowerInvariant().Contains("list all") || 
                    message.ToLowerInvariant().Contains("toon alles") || message.ToLowerInvariant().Contains("lijst alles"))
                {
                    var restaurantData = state.Places.Select(p => new { p.Name, p.Cuisine }).ToList();
                    var restaurantJson = JsonSerializer.Serialize(restaurantData);
                    
                    var showAllPrompt = lang == "nl"
                        ? $"De gebruiker wil alle {state.Places.Count} restaurants zien. Hier is de data: {restaurantJson}. Presenteer ze genummerd en enthousiast. Leg uit dat ze een nummer kunnen antwoorden voor details."
                        : $"The user wants to see all {state.Places.Count} restaurants. Here's the data: {restaurantJson}. Present them numbered and enthusiastically. Explain they can reply with a number for details.";
                    
                    var showAllResponse = await _aiService.GetRestaurantSearchResponseAsync(
                        showAllPrompt,
                        state.ChatHistory,
                        null,
                        restaurantData.Cast<object>().ToList()
                    );
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(showAllResponse));
                    
                    return Ok(new ChatResponse { Reply = showAllResponse, SessionId = sessionId, Data = restaurantData });
                }

                // If user typed a number, show details
                if (int.TryParse(message.Split(' ').FirstOrDefault() ?? "", out var idx))
                {
                    if (idx >= 1 && idx <= state.Places.Count)
                    {
                        var p = state.Places[idx - 1];
                        state.Step = ConversationStep.AwaitingConfirmation;
                        
                        var detailsPrompt = lang == "nl"
                            ? $"De gebruiker wil details van restaurant nummer {idx}: {p.Name}, Keuken: {p.Cuisine}, Adres: {p.Address}. Presenteer deze informatie op een aantrekkelijke manier met emoji's. Vraag of ze willen boeken ('boek'), een andere optie willen zien ('andere'), of terug willen naar de lijst ('terug')."
                            : $"The user wants details of restaurant number {idx}: {p.Name}, Cuisine: {p.Cuisine}, Address: {p.Address}. Present this information in an appealing way with emojis. Ask if they'd like to book ('book'), see another option ('another'), or go back to the list ('back').";
                        
                        var detailsResponse = await _aiService.GetResponseAsync(detailsPrompt, state.ChatHistory);
                        state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(detailsResponse));
                        
                        return Ok(new ChatResponse { Reply = detailsResponse, SessionId = sessionId });
                    }
                    else
                    {
                        var invalidNumberPrompt = lang == "nl"
                            ? $"De gebruiker koos nummer {idx}, maar er zijn maar {state.Places.Count} restaurants. Leg dit vriendelijk uit."
                            : $"The user chose number {idx}, but there are only {state.Places.Count} restaurants. Explain this kindly.";
                        
                        var invalidResponse = await _aiService.GetResponseAsync(invalidNumberPrompt, state.ChatHistory);
                        state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(invalidResponse));
                        
                        return Ok(new ChatResponse { Reply = invalidResponse, SessionId = sessionId });
                    }
                }

                // Treat as cuisine filter
                var queryCuisine = message.ToLowerInvariant();
                var filtered = state.Places.Where(p => 
                    (!string.IsNullOrEmpty(p.Cuisine) && p.Cuisine.ToLowerInvariant().Contains(queryCuisine)) || 
                    p.Name.ToLowerInvariant().Contains(queryCuisine)
                ).ToList();
                
                if (filtered.Count == 0)
                {
                    var cuisines = string.Join(", ", state.Places.Where(p => !string.IsNullOrEmpty(p.Cuisine) && p.Cuisine != "Various").Select(p => p.Cuisine).Distinct());
                    
                    var noMatchPrompt = lang == "nl"
                        ? $"Er zijn geen restaurants die matchen met '{message}'. Beschikbare keukens: {cuisines}. Stel vriendelijk voor om een andere keuken te proberen of 'toon alles' te typen."
                        : $"No restaurants match '{message}'. Available cuisines: {cuisines}. Kindly suggest trying another cuisine or typing 'show all'.";
                    
                    var noMatchResponse = await _aiService.GetResponseAsync(noMatchPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(noMatchResponse));
                    
                    return Ok(new ChatResponse { Reply = noMatchResponse, SessionId = sessionId });
                }

                state.Places = filtered;
                var filteredData = filtered.Select(p => new { p.Name, p.Cuisine }).ToList();
                var filteredJson = JsonSerializer.Serialize(filteredData);
                
                var filterPrompt = lang == "nl"
                    ? $"We hebben {filtered.Count} {message} restaurant(s) gevonden. Hier is de data: {filteredJson}. Presenteer ze enthousiast en genummerd. Leg uit dat ze een nummer kunnen antwoorden voor details."
                    : $"We found {filtered.Count} {message} restaurant(s). Here's the data: {filteredJson}. Present them enthusiastically and numbered. Explain they can reply with a number for details.";
                
                var filterResponse = await _aiService.GetRestaurantSearchResponseAsync(
                    filterPrompt,
                    state.ChatHistory,
                    null,
                    filteredData.Cast<object>().ToList()
                );
                state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(filterResponse));
                
                return Ok(new ChatResponse { Reply = filterResponse, SessionId = sessionId, Data = filteredData });
            }

            // Step 4: Booking confirmation with AI
            if (state.Step == ConversationStep.AwaitingConfirmation)
            {
                var lower = message.ToLowerInvariant();
                
                if (lower.Contains("book") || lower.Contains("confirm") || lower.Contains("yes") || lower.Contains("reserve") ||
                    lower.Contains("boek") || lower.Contains("bevestig") || lower.Contains("ja") || lower.Contains("reserveer"))
                {
                    var confirmPrompt = lang == "nl"
                        ? "De gebruiker wil het restaurant boeken. Feliciteer hen enthousiast! Leg uit dat hun boekingsverzoek is genoteerd en dat het restaurant hen zal contacteren. Vermeld dat dit een demo is en vraag of ze nog een ander restaurant willen zoeken."
                        : "The user wants to book the restaurant. Congratulate them enthusiastically! Explain their booking request has been noted and the restaurant will contact them. Mention this is a demo and ask if they'd like to search for another restaurant.";
                    
                    var confirmResponse = await _aiService.GetResponseAsync(confirmPrompt, state.ChatHistory);
                    Sessions.TryRemove(sessionId, out _);
                    
                    return Ok(new ChatResponse { Reply = confirmResponse, SessionId = sessionId });
                }

                if (lower.Contains("another") || lower.Contains("no") || lower.Contains("different") ||
                    lower.Contains("andere") || lower.Contains("nee") || lower.Contains("verschillend"))
                {
                    state.Step = ConversationStep.AwaitingCuisine;
                    
                    var anotherPrompt = lang == "nl"
                        ? "De gebruiker wil een andere optie zien. Bevestig dit vriendelijk en vraag of ze willen filteren op keuken of alle restaurants willen zien."
                        : "The user wants to see another option. Acknowledge this kindly and ask if they'd like to filter by cuisine or see all restaurants.";
                    
                    var anotherResponse = await _aiService.GetResponseAsync(anotherPrompt, state.ChatHistory);
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(anotherResponse));
                    
                    return Ok(new ChatResponse { Reply = anotherResponse, SessionId = sessionId });
                }

                if (lower.Contains("back") || lower.Contains("list") ||
                    lower.Contains("terug") || lower.Contains("lijst"))
                {
                    state.Step = ConversationStep.AwaitingCuisine;
                    
                    var restaurantData = state.Places.Select(p => new { p.Name, p.Cuisine }).ToList();
                    var restaurantJson = JsonSerializer.Serialize(restaurantData);
                    
                    var backPrompt = lang == "nl"
                        ? $"De gebruiker wil terug naar de volledige lijst. Hier is de data: {restaurantJson}. Presenteer de lijst opnieuw op een vriendelijke manier en leg uit dat ze een nummer kunnen antwoorden voor details."
                        : $"The user wants to go back to the full list. Here's the data: {restaurantJson}. Present the list again in a friendly way and explain they can reply with a number for details.";
                    
                    var backResponse = await _aiService.GetRestaurantSearchResponseAsync(
                        backPrompt,
                        state.ChatHistory,
                        null,
                        restaurantData.Cast<object>().ToList()
                    );
                    state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(backResponse));
                    
                    return Ok(new ChatResponse { Reply = backResponse, SessionId = sessionId, Data = restaurantData });
                }

                // Didn't understand
                var confusedPrompt = lang == "nl"
                    ? "De gebruiker's antwoord was onduidelijk. Leg vriendelijk uit dat ze kunnen: 'boek' - om te reserveren, 'andere' - voor andere opties, 'terug' - voor de lijst."
                    : "The user's answer was unclear. Kindly explain they can: 'book' - to reserve, 'another' - for other options, 'back' - for the list.";
                
                var confusedResponse = await _aiService.GetResponseAsync(confusedPrompt, state.ChatHistory);
                state.ChatHistory.Add(ChatMessage.CreateAssistantMessage(confusedResponse));
                
                return Ok(new ChatResponse { Reply = confusedResponse, SessionId = sessionId });
            }

            // Fallback with AI
            var fallbackPrompt = lang == "nl"
                ? "Er is iets misgegaan in het gesprek. Stel vriendelijk voor om opnieuw te beginnen en vraag in welk land ze willen zoeken naar restaurants."
                : "Something went wrong in the conversation. Kindly suggest starting over and ask which country they'd like to search for restaurants in.";
            
            var fallbackResponse = await _aiService.GetResponseAsync(fallbackPrompt, state.ChatHistory);
            
            return Ok(new ChatResponse { Reply = fallbackResponse, SessionId = sessionId });
        }
    }
}
