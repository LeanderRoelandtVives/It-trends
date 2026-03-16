# Restaurant Search - Geolocation & Location Troubleshooting Guide

## Issue: Geolocation Blocked

You're seeing this error:
```
? Geolocation error: 1 User denied Geolocation
Geolocation permission has been blocked as the user has ignored the permission prompt several times.
```

This means you clicked "Block" or dismissed the geolocation prompt multiple times. The good news is **you don't need geolocation** to search for restaurants!

---

## Solution 1: Search by City Name (Recommended) ?

Instead of using "near me", just **mention the city name**:

### Try these searches:
- ? **"Restaurants in Amsterdam"**
- ? **"Pizza in London"**
- ? **"Italian restaurants in Paris"**
- ? **"Cheap food in Barcelona"**
- ? **"Seafood in Berlin"**

**This works perfectly and doesn't require location permission!**

---

## Solution 2: Reset Browser Location Permission

If you want "near me" to work, you need to reset the blocked permission:

### In Google Chrome:
1. Click the **?? lock icon** next to the URL in the address bar
2. Find **"Location"** in the popup
3. Click the **"Clear"** or **"Reset"** button
4. Refresh the page
5. When asked "Do you want to share your location?", click **"Allow"**

### In Firefox:
1. Click the **?? lock icon** in the address bar
2. Find "Permissions" section
3. Click the **"X"** next to "Location" to remove the block
4. Refresh the page
5. Click **"Allow"** when prompted

### In Safari:
1. Go to **Safari ? Preferences ? Privacy**
2. Find **"Location Services"**
3. Find this website and change to **"Ask"** or **"Allow"**
4. Refresh the page

### In Edge:
1. Click the **?? lock icon** in the address bar
2. Find **"Location"** and click to change permission
3. Select **"Allow"**
4. Refresh the page

---

## How the Restaurant Search Works Now

### **Path 1: Location-Based Search** (Requires mentioning city)
```
User: "Italian restaurants in Amsterdam"
   ?
API: Extracts location "Amsterdam"
   ?
API: Geocodes to coordinates (52.37°N, 4.89°E)
   ?
API: Queries Overpass for restaurants in that area
   ?
Result: 5 restaurants with Google Maps links
```

### **Path 2: Geolocation Search** (Requires location permission)
```
User: "Find restaurants near me"
   ?
Browser: Requests user's GPS location
   ?
User: Clicks "Allow"
   ?
API: Uses coordinates from browser
   ?
API: Queries Overpass for restaurants within 3km
   ?
Result: 5 restaurants with Google Maps links
```

### **Path 3: Default Search** (No inputs)
```
User: Just mentions "restaurant"
   ?
API: Uses default Amsterdam location
   ?
Result: 5 restaurants in Amsterdam
```

---

## What Works Right Now ?

These queries **definitely work** without location permission:

```
"Restaurants in Amsterdam"
"Pizza in London"
"Burger restaurants in Paris"
"Italian food in Berlin"
"Cheap meals in Barcelona"
"Chinese restaurants in Brussels"
"Seafood in Rome"
"Vegetarian restaurants in Vienna"
"Fast food in Madrid"
"Cafe in Prague"
```

---

## How to Find Restaurant Results

### Expected Output Format:
```
Here are some restaurants I found:

1. Restaurant Name • Location
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name

2. Restaurant Name • Location
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name

3. Restaurant Name • Location
   https://www.google.com/maps/search/?api=1&query=Restaurant+Name

(etc.)

Would you like more details or to make a reservation?
```

Each result includes a **clickable Google Maps link** to see the restaurant on a map.

---

## Expanded Search Keywords ?

The chatbot now recognizes:
- ? "restaurant" / "restaurants"
- ? "pizza" / "pizzeria"
- ? "food" / "eating"
- ? "cuisine"
- ? "dinner"
- ? "lunch"
- ? "near me" / "nearby"
- ? "find" / "search"
- ? "in" / "at" / "around"

Combined with location names like:
- Amsterdam, London, Paris, Berlin, Barcelona, Brussels, Rome, Vienna, Madrid, Prague, etc.

---

## Troubleshooting: Still Not Getting Results?

### 1. **Try a bigger/more famous city:**
Instead of: "Restaurants in SmallTown"
Try: "Restaurants in Amsterdam"

### 2. **Be more specific with cuisine:**
Instead of: "Find food"
Try: "Italian restaurants in Paris"

### 3. **Check the API is running:**
Make sure your API server is running on port 7179:
```
https://localhost:7179/api/AiChat/message
```

### 4. **Check the browser console:**
Press `F12` and check for error messages. Look for:
- ? Green log: "Geolocation captured: {lat, lng}"
- ?? Yellow log: "Geolocation error: 1 User denied" (This is OK - use city names instead)
- ? Red error: "Network error" (API might not be running)

### 5. **Clear browser cache:**
- Press `Ctrl+Shift+Delete` (Chrome) or `Cmd+Shift+Delete` (Mac)
- Clear "Cookies and other site data"
- Reload the page

---

## Database/External Services Used

- **Overpass API**: Free, open-source database of world map data (from OpenStreetMap)
- **Nominatim**: Free geocoding service (converts city names to coordinates)
- **Google Maps**: Provides map links (when you click the restaurant link)

All are **free** and don't require API keys! ?

---

## What Happens Behind the Scenes

1. **Location Extraction**: AI identifies location from your message (e.g., "Amsterdam" from "Italian restaurants in Amsterdam")
2. **Geocoding**: Converts "Amsterdam" to GPS coordinates using Nominatim
3. **Overpass Query**: Builds a query to find restaurants in that area
4. **Parsing Results**: Extracts restaurant names and coordinates
5. **Formatting**: Creates clickable Google Maps links
6. **Response**: Shows you up to 5 results with a friendly message

---

## Still Have Questions?

**Common Questions:**

**Q: Will "near me" ever work?**
A: Yes! Reset your location permission (see Solution 2 above) and try again.

**Q: Do I need an API key?**
A: No! The search uses free, open APIs (Overpass + Nominatim).

**Q: Why only 5 results?**
A: Overpass API is rate-limited for performance. 5 results should be enough to get started!

**Q: Can I search without mentioning a city?**
A: Yes, but it defaults to Amsterdam. Mentioning a city gives better results.

**Q: What if I make a typo in the city name?**
A: The Nominatim geocoder is pretty good at handling typos, but try spelling common cities correctly.

---

## Summary

? **Works great:** Search by city name
```
"Italian restaurants in Amsterdam"
"Pizza in London"
"Cheap food in Paris"
```

?? **Requires permission:** Search by location
```
"Find restaurants near me"
(Requires clicking "Allow" on geolocation prompt)
```

?? **No setup needed:** All free APIs, no configuration required!

Happy searching! ??????
