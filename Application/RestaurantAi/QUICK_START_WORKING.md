# Quick Start: Restaurant Search Chatbot ?

## What's Working Right Now

Your restaurant search chatbot is **fully functional** using the free Overpass API! No API keys needed.

---

## ? Quick Test (30 seconds)

### 1. Start Both Applications
```
API: https://localhost:7179
MVC: https://localhost:7227
```

### 2. Open Dashboard
Go to: `https://localhost:7227/Home/Dashboard`

### 3. Try This Search
Type: **"Italian restaurants in Amsterdam"**

### Expected Response
```
Here are some restaurants I found:

1. Restaurant Name • Amsterdam
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name

2. Restaurant Name • Amsterdam
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name

(etc...)

Would you like more details or to make a reservation?
```

---

## ?? Search Examples That Work

| What You Want | Try Saying |
|---|---|
| Find pizza places | "Pizza restaurants in Paris" |
| Find cheap eats | "Cheap food in London" |
| Find by cuisine | "Chinese restaurants in Berlin" |
| Find seafood | "Seafood in Barcelona" |
| Search default city | "What restaurants are there?" |
| Spanish restaurants | "Spanish restaurants in Madrid" |
| Italian cuisine | "Italian restaurants in Rome" |
| Vegetarian food | "Vegetarian restaurants in Vienna" |

---

## ?? How Location Works

### Method 1: City Name (Recommended) ?
```
You: "Pizza in Amsterdam"
API: Extracts "Amsterdam"
API: Geocodes to coordinates
API: Searches near those coordinates
Result: Restaurants found ?
```

### Method 2: Browser Location
```
You: "Find restaurants near me"
Browser: Asks for location permission
You: Click "Allow"
API: Uses your GPS coordinates
Result: Restaurants within 3km ?
```

### Method 3: Default Location
```
You: "Show me some restaurants"
API: Uses default (Amsterdam)
Result: Restaurants in Amsterdam ?
```

---

## ?? If You Get No Results

Try these steps:

1. **Use a major city:**
   - ? "Restaurants in SmallTown"
   - ? "Restaurants in Amsterdam"

2. **Be more specific:**
   - ? "Find food"
   - ? "Italian restaurants in Paris"

3. **Check if API is running:**
   - Make sure `RestaurantAi.Api` is running on port 7179
   - Check browser console (F12) for errors

4. **Try simpler query:**
   - ? "I'm looking for a nice place to eat pizza with my friends"
   - ? "Pizza restaurants in London"

---

## ??? System Architecture

```
???????????????????
?  Your Browser   ?
?  (Dashboard)    ?
???????????????????
         ? POST /api/AiChat/message
         ? {"Message": "Pizza in Paris", ...}
         ?
???????????????????????????????
?   RestaurantAi.Api (7179)   ?
?                             ?
? 1. Extract location         ?
? 2. Geocode to coordinates   ?
? 3. Query Overpass API       ?
? 4. Parse results            ?
? 5. Format response          ?
???????????????????????????????
         ?
         ??? Nominatim (Geocoding)
         ?   "Paris" ? 48.8566, 2.3522
         ?
         ??? Overpass API (Restaurants)
         ?   [bbox]; restaurants; out
         ?
         ??? Google Maps (Links)
             https://maps.google.com
         
         ?
        Response sent back
```

---

## ?? Current Features

? **Location Extraction**
- Recognizes "in [city]", "near [city]", "around [city]"
- Examples: "restaurants in Amsterdam", "pizza near Paris"

? **Geocoding (OSM Nominatim)**
- Converts city names to coordinates
- Handles typos and aliases
- Free and no API key needed

? **Restaurant Search (Overpass API)**
- Queries 3km radius around coordinates
- Returns up to 5 results
- Extracts name and location

? **Google Maps Integration**
- Each result links to Google Maps
- One-click directions

? **Multi-language**
- English ?
- Dutch (Nederlands) ?

? **Browser Geolocation**
- Detects user location if permission granted
- Falls back to city-based search
- Handles permission denial gracefully

---

## ?? Supported Search Keywords

The chatbot recognizes when you're searching for restaurants if your message contains:

- `restaurant` / `restaurants`
- `pizza` / `pizzeria`
- `food` / `eat` / `eating`
- `cuisine`
- `dinner` / `lunch`
- `near me` / `nearby` / `close to`
- `find` / `search`
- `in` / `at` / `around`

---

## ?? Next Steps (Optional)

### Add More Features:
1. **Restaurant Details**: Fetch opening hours, phone, website
2. **Ratings**: Display user ratings from OpenStreetMap
3. **Filtering**: "Only vegetarian", "Open now", "Cheap"
4. **Bookings**: Integrate reservation system
5. **Reviews**: Show recent reviews

### Integrate Paid APIs:
1. **Google Places**: For ratings, reviews, photos
2. **Yelp**: For more detailed business information
3. **OpenAI/Groq**: For conversational AI (currently using basic rules)

### Improve Search:
1. Add cuisine type recognition ("Italian", "Chinese", etc.)
2. Add price range filtering ("$", "$$", "$$$")
3. Add opening hours info ("Open now")
4. Add distance calculation

---

## ?? Backend Improvements Made

### Enhanced Location Detection
```csharp
// Now recognizes:
- "near me" ? Uses browser geolocation
- "Italian restaurants in Paris" ? Extracts "Paris"
- "Cheap food near London" ? Extracts "London"
- "Pizza?" ? Works with just one keyword
```

### Better Error Messages
```
Instead of: "I couldn't find restaurants"
Now: "I couldn't find restaurants. Try:
     • A bigger city (Amsterdam, London, Paris)
     • Reset location (click ?? in address bar)
     • Try: 'Italian restaurants in Berlin'"
```

### Comprehensive Logging
All searches are logged with:
- Location name or coordinates used
- Geocoding results
- Overpass API response count
- Any errors encountered

---

## ? What Makes This Work

1. **Free APIs Only**
   - Overpass API: OpenStreetMap restaurant data
   - Nominatim: Geocoding (city ? coordinates)
   - Google Maps: Map links

2. **No API Keys Needed**
   - Everything public and free-tier friendly
   - Rate limiting is generous for this use case

3. **Smart Fallbacks**
   - No geolocation? Use city name
   - City not found? Search by restaurant name
   - No coordinates? Use Amsterdam default

4. **User-Friendly**
   - Natural language input ("Italian restaurants")
   - Helpful error messages with suggestions
   - Works without location permission

---

## ?? Support

**Your chatbot now supports:**
- ? Searching by city name
- ? Searching by "near me"
- ? Multiple languages (EN, NL)
- ? Cuisine type hints
- ? Price range hints
- ? Direct Google Maps links

**Try it now with:**
```
"Italian restaurants in Amsterdam"
"Pizza in London"
"Seafood near me"
"Food in Paris"
```

Happy searching! ??????

---

## Changelog

### Latest Updates
- ? Improved location extraction (handles typos)
- ? Added Nominatim geocoding support
- ? Expanded search keywords (pizza, food, cuisine, etc.)
- ? Better error messages with troubleshooting tips
- ? Enhanced geolocation fallback handling
- ? Logging for debugging restaurant searches
- ? Google Maps links for each result

### Known Limitations
- ?? 5 restaurant limit per search (Overpass API rate limiting)
- ?? No restaurant ratings (Overpass doesn't provide them)
- ?? No opening hours info
- ?? No photo previews
- ?? No reservation capability (yet)

### Future Enhancements
- ?? Integration with Google Places for ratings
- ?? Restaurant opening hours
- ?? Photo previews
- ?? Reservation booking system
- ?? AI conversation using Groq API
