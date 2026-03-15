# Restaurant Search API - Complete Reference

## Base Endpoint
```
POST https://localhost:7179/api/AiChat/message
```

---

## Request Format

### Headers
```
Content-Type: application/json
```

### Body
```json
{
  "Message": "Italian restaurants in Amsterdam",
  "SessionId": "abc123def456",
  "Language": "en",
  "Latitude": null,
  "Longitude": null
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Message` | string | ? Yes | User's input message |
| `SessionId` | string | ? Yes | Session identifier (can be empty string for new session) |
| `Language` | string | ? Yes | Language code: `"en"` or `"nl"` |
| `Latitude` | double? | ? No | User's latitude (from geolocation) |
| `Longitude` | double? | ? No | User's longitude (from geolocation) |

---

## Response Format

### Success Response (200 OK)
```json
{
  "reply": "Here are some restaurants I found:\n\n1. Restaurant Name • Amsterdam\n   https://www.google.com/maps/search/?api=1&query=Restaurant+Name\n\nWould you like more details or to make a reservation?",
  "sessionId": "abc123def456"
}
```

### Welcome Message (Empty Message)
```json
{
  "reply": "Hi! ?? I'm your restaurant concierge AI. How can I help?\n\nTry: 'Restaurants in Amsterdam', 'Pizza near me', 'Cheap food in London'\n\n(If 'near me' doesn't work, you may have blocked location. Check the ?? icon in your address bar to enable it.)",
  "sessionId": "new_session_id_generated"
}
```

### No Results Response
```json
{
  "reply": "I couldn't find restaurants for that location.\n\nTry:\n• Mentioning a different city (e.g., 'restaurants in London')\n• Enabling location (click ?? in the address bar)\n• Specifying a cuisine type (e.g., 'Italian restaurants')",
  "sessionId": "abc123def456"
}
```

### Non-Restaurant Query
```json
{
  "reply": "I understood: \"Hello\". Try: \"Find restaurants near me\" or \"Italian restaurants in Amsterdam\".",
  "sessionId": "abc123def456"
}
```

### Error Response (500)
```json
{
  "reply": "Sorry, something went wrong. Please try again.",
  "sessionId": "abc123def456"
}
```

---

## Search Flow Diagram

```
???????????????????????????????????????
?  User Input: "Italian in Paris"     ?
???????????????????????????????????????
             ?
             ?
    ??????????????????????
    ? Check if searching ?
    ? for restaurants?   ?
    ??????????????????????
             ?
    ??????????????????????
    ? YES               NO
    ?                   ?
    ?                   ?
    ?           ????????????????????
    ?           ? Return fallback  ?
    ?           ? message          ?
    ?           ????????????????????
    ?
    ?
???????????????????????????????
? Extract Location            ?
? Input: "Italian in Paris"   ?
? Output: "Paris"             ?
???????????????????????????????
         ?
         ?
    ???????????????????????????????????????
    ? Decision: Which search method?      ?
    ???????????????????????????????????????
         ?         ?           ?
    ?????????  ?????????  ??????????
    ?Has    ?  ?Has    ?  ?Has     ?
    ?Coords??  ?Loc?   ?  ?Neither??
    ? YES   ?  ? YES   ?  ? YES    ?
    ?????????  ?????????  ??????????
         ?        ?            ?
         ?        ?            ?
    ??????????????????????????????????
    ? Method 1: Use coordinates      ?
    ? (3km radius bbox query)        ?
    ??????????????????????????????????
         
    ??????????????????????????????????
    ? Method 2: Geocode location     ?
    ? "Paris" ? (48.8566, 2.3522)   ?
    ? Then search by coordinates     ?
    ??????????????????????????????????
         
    ??????????????????????????????????
    ? Method 3: Use default Amsterdam?
    ? (52.3602, -4.8952)            ?
    ??????????????????????????????????
         ?        ?            ?
         ???????????????????????
              ?            ?
              ?            ?
    ???????????????????????????????????
    ? Query Overpass API              ?
    ? [out:json];                     ?
    ? (                               ?
    ?   node[amenity=restaurant]      ?
    ?   way[amenity=restaurant]       ?
    ?   relation[amenity=restaurant]  ?
    ? );                              ?
    ? out center 20;                  ?
    ???????????????????????????????????
               ?
               ?
    ???????????????????????????????????
    ? Parse Results                   ?
    ? Extract: Name, Coordinates      ?
    ? Limit: 5 results max            ?
    ???????????????????????????????????
               ?
               ?
    ????????????????????????????????????
    ? Format Response                  ?
    ? Build Google Maps links          ?
    ? Add helpful message              ?
    ????????????????????????????????????
               ?
               ?
    ????????????????????????????????????
    ? Return to Client                 ?
    ? {reply, sessionId}               ?
    ????????????????????????????????????
```

---

## Search Recognition Keywords

The API recognizes a search request if the message contains any of:

```
restaurant  / restaurants
pizza       / pizzeria
food        / eat / eating
cuisine
dinner      / lunch
near me     / nearby / close to me
find        / search
in          / at / near / around
```

### Examples That Trigger Search

? "restaurants in Amsterdam"
? "pizza near me"
? "find food in london"
? "italian cuisine"
? "where to eat"
? "dinner places"
? "restaurants nearby"
? "pizzeria in paris"

### Examples That DON'T Trigger Search

? "hello"
? "how are you"
? "what time is it"
? "tell me a joke"

---

## Language Support

### English (`language: "en"`)

**Welcome:**
```
Hi! ?? I'm your restaurant concierge AI. How can I help?

Try: 'Restaurants in Amsterdam', 'Pizza near me', 'Cheap food in London'

(If 'near me' doesn't work, you may have blocked location. Check the ?? icon in your address bar to enable it.)
```

**Results Header:**
```
Here are some restaurants I found:
```

**Result Format:**
```
1. Restaurant Name • Location
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name
```

**Results Footer:**
```
Would you like more details or to make a reservation?
```

**No Results:**
```
I couldn't find restaurants for that location.

Try:
• Mentioning a different city (e.g., 'restaurants in London')
• Enabling location (click ?? in the address bar)
• Specifying a cuisine type (e.g., 'Italian restaurants')
```

**Non-Restaurant Query:**
```
I understood: "[user input]". Try: "Find restaurants near me" or "Italian restaurants in Amsterdam".
```

---

### Dutch (`language: "nl"`)

**Welcome:**
```
Hoi! ?? Ik ben je restaurant conciërge AI. Waar kan ik je mee helpen?

Vb: 'Restaurants in Amsterdam', 'Pizza dicht bij mij', 'Goedkope eetgelegenheden in Brussel'
```

**Results Header:**
```
Ik heb de volgende restaurants gevonden:
```

**Result Format:**
```
1. Restaurant Name • Location
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name
```

**Results Footer:**
```
Wil je dat ik meer details geef of een reservering maak?
```

**No Results:**
```
Kon geen restaurants vinden voor die locatie.

Probeer:
• Een andere plaats noemen (bv. 'restaurants in Amsterdam')
• De locatiegegevens in te schakelen (klik ?? in de adresbalk)
• Een restauranttype te noemen (bv. 'Italiaanse restaurants')
```

**Non-Restaurant Query:**
```
Ik begreep: "[user input]". Probeer bijvoorbeeld: "Vind restaurants bij mij in de buurt" of "Italiaanse restaurants in Amsterdam".
```

---

## Location Extraction Logic

### Input Processing
The API processes queries in this order:

1. **Check for "near me" variants:**
   - Contains "near me"
   - Contains "nearby"
   - Contains "close to me"
   - ? Returns `null` ? Uses geolocation

2. **Check for location keywords:**
   - Contains " in "
   - Contains " near "
   - Contains " around "
   - Contains " by "
   - ? Extracts text after keyword

3. **Cuisine + Location pattern:**
   - Finds "restaurants", "restaurant", "food", "pizzeria", "cafe"
   - Looks for " in "/" near "/" at " after
   - ? Extracts location part

4. **Fallback:**
   - No location found
   - ? Returns `null` ? Uses default (Amsterdam)

### Examples

| Input | Extracted | Method |
|-------|-----------|--------|
| "restaurants near me" | null | Geolocation |
| "pizza in amsterdam" | "amsterdam" | " in " pattern |
| "italian in paris" | "paris" | " in " pattern |
| "food near london" | "london" | " near " pattern |
| "pizzeria around berlin" | "berlin" | " around " pattern |
| "restaurants" | null | Default |
| "find food" | null | Default |

---

## Geocoding (Nominatim)

### Process
1. User says: "Italian restaurants in Paris"
2. Location extracted: "Paris"
3. Nominatim query: 
   ```
   https://nominatim.openstreetmap.org/search?q=Paris&format=json&limit=1
   ```
4. Response:
   ```json
   [
     {
       "lat": "48.8566",
       "lon": "2.3522",
       ...
     }
   ]
   ```
5. Coordinates used: `(48.8566, 2.3522)`

### Supported Cities
- All cities in OpenStreetMap database
- Handles typos reasonably well
- Returns first match if multiple cities exist

---

## Overpass API Query

### Bbox Query (with coordinates)
```
[out:json];
(
  node[amenity=restaurant](south,west,north,east);
  way[amenity=restaurant](south,west,north,east);
  relation[amenity=restaurant](south,west,north,east);
);
out center 20;
```

### Name Query (without coordinates)
```
[out:json];
(
  node[amenity=restaurant][name~"query",i];
  way[amenity=restaurant][name~"query",i];
  relation[amenity=restaurant][name~"query",i];
);
out center 20;
```

### Response Structure
```json
{
  "elements": [
    {
      "type": "node",
      "id": 123456,
      "lat": 48.8566,
      "lon": 2.3522,
      "tags": {
        "name": "Restaurant Name",
        "amenity": "restaurant"
      }
    },
    {
      "type": "way",
      "id": 654321,
      "center": {
        "lat": 52.3602,
        "lon": 4.8952
      },
      "tags": {
        "name": "Another Restaurant",
        "amenity": "restaurant"
      }
    }
  ]
}
```

---

## Logging Output

All searches are logged in the API console:

```
info: AiChatController - Searching restaurants by location: Paris
info: AiChatController - Found coordinates for Paris: 48.8566, 2.3522
info: AiChatController - Found 5 restaurants for location Paris
```

**Error cases:**
```
warn: AiChatController - Could not geocode location: InvalidCity
warn: AiChatController - Overpass API returned 429 for location paris
```

---

## Rate Limits

### Nominatim (Geocoding)
- Max 1 request per second
- Recommended: 1 second delay between requests

### Overpass API
- Max 1 request per second
- Shared resource, please be respectful
- Fallback to name-based search if rate limited

### Google Maps Links
- No limits (client-side only)

---

## Error Handling

### Network Errors
If Nominatim or Overpass are unavailable:
```
?? API falls back gracefully
? Returns helpful suggestion to user
? Logs the error for debugging
```

### No Results
If coordinates yield no restaurants:
```
? Returns "I couldn't find restaurants" message
? Suggests trying a different city
? Guides user on location permission
```

### Malformed Request
If request is missing required fields:
```
HTTP 400 Bad Request
```

---

## Testing

### Test Case 1: City Search
```json
POST /api/AiChat/message
{
  "Message": "Italian restaurants in Amsterdam",
  "SessionId": "",
  "Language": "en",
  "Latitude": null,
  "Longitude": null
}
```

### Test Case 2: Geolocation Search
```json
POST /api/AiChat/message
{
  "Message": "Find restaurants near me",
  "SessionId": "",
  "Language": "en",
  "Latitude": 52.3602,
  "Longitude": 4.8952
}
```

### Test Case 3: Default Search
```json
POST /api/AiChat/message
{
  "Message": "What restaurants are available?",
  "SessionId": "",
  "Language": "en",
  "Latitude": null,
  "Longitude": null
}
```

### Test Case 4: Non-Restaurant Query
```json
POST /api/AiChat/message
{
  "Message": "Hello, how are you?",
  "SessionId": "abc123",
  "Language": "en",
  "Latitude": null,
  "Longitude": null
}
```

---

## Integration Examples

### JavaScript/Fetch
```javascript
const response = await fetch('https://localhost:7179/api/AiChat/message', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    Message: "Pizza in London",
    SessionId: sessionId,
    Language: "en",
    Latitude: null,
    Longitude: null
  })
});

const data = await response.json();
console.log(data.reply);
```

### cURL
```bash
curl -X POST https://localhost:7179/api/AiChat/message \
  -H "Content-Type: application/json" \
  -d '{
    "Message": "Restaurants in Paris",
    "SessionId": "",
    "Language": "en",
    "Latitude": null,
    "Longitude": null
  }'
```

### C# / HttpClient
```csharp
var client = new HttpClient();
var request = new
{
    Message = "Italian restaurants in Berlin",
    SessionId = "",
    Language = "en",
    Latitude = (double?)null,
    Longitude = (double?)null
};

var content = new StringContent(
    JsonSerializer.Serialize(request),
    Encoding.UTF8,
    "application/json"
);

var response = await client.PostAsync(
    "https://localhost:7179/api/AiChat/message",
    content
);

var result = await response.Content.ReadAsAsync<ChatResponse>();
```

---

## External APIs Used

### 1. Nominatim (OpenStreetMap)
- **Purpose**: Geocoding (city name ? coordinates)
- **Free Tier**: ? Yes
- **Rate Limit**: 1 request/second
- **No API Key**: ? Required

### 2. Overpass API
- **Purpose**: Restaurant database query
- **Free Tier**: ? Yes
- **Rate Limit**: 1 request/second
- **No API Key**: ? Required

### 3. Google Maps
- **Purpose**: Links to restaurant locations
- **Free Tier**: ? Yes (links only)
- **Rate Limit**: None (static links)
- **No API Key**: ? Required

---

## Performance Metrics

- **Average Response Time**: 1-3 seconds
  - Nominatim: ~500ms
  - Overpass: ~1-2s
  - Parsing: ~100ms

- **Search Limit**: 5 results per query (configurable)

- **Timeout**: 15 seconds per request

---

## Troubleshooting API Issues

### Issue: 500 Error
**Solution**: Check API logs for specific error. Likely causes:
- Network timeout (Nominatim/Overpass unreachable)
- Invalid JSON in request
- Missing required fields

### Issue: Empty Results
**Solution**: 
- Try a major city name
- Check if location name is spelled correctly
- Verify internet connection (external APIs)

### Issue: Slow Response
**Solution**:
- Overpass API may be busy
- Nominatim geocoding is slow (~500ms)
- Wait a moment and retry

### Issue: Coordinates Not Working
**Solution**:
- Ensure latitude/longitude are valid (-90 to 90, -180 to 180)
- Try with a city name instead
- Check browser geolocation permission

---

## Future Enhancements

?? **Planned Features**:
- Rating display (from Overpass tags)
- Opening hours integration
- Photo previews
- Reservation booking
- AI conversation using Groq API
- Restaurant type filtering
- Price range filtering
- Distance calculation

---

**Last Updated**: 2024
**API Version**: 1.0
**Status**: ? Production Ready
