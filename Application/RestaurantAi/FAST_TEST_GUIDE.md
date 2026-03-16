# Quick Test: Get Restaurants Working

## ? Fastest Way to Fix This

### Option 1: Test Amsterdam (Should Always Work)

1. **Clear browser cache**
   - Press `Ctrl+Shift+Delete`
   - Select "Cookies and other site data"
   - Clear

2. **Refresh dashboard**
   - `https://localhost:7227/Home/Dashboard`

3. **Try this exact message**
   ```
   restaurants in amsterdam
   ```

4. **Check Visual Studio Output Window**
   - Debug ? Windows ? Output
   - Look for: "Found coordinates for amsterdam: 52.37, 4.89"
   - Look for: "Overpass returned X elements"

5. **If no results appear:**
   - Copy log message
   - Share it below

---

### Option 2: Direct API Test with Postman/cURL

**Using cURL:**
```bash
curl -X POST https://localhost:7179/api/AiChat/message \
  -H "Content-Type: application/json" \
  -d '{
    "Message": "restaurants in amsterdam",
    "SessionId": "",
    "Language": "en",
    "Latitude": null,
    "Longitude": null
  }'
```

**Expected Response:**
```json
{
  "reply": "Here are some restaurants I found:\n\n1. Restaurant Name • amsterdam\n...",
  "sessionId": "abc123..."
}
```

---

### Option 3: Test Nominatim & Overpass Separately

**Test Nominatim (Geocoding):**
```bash
# In browser address bar:
https://nominatim.openstreetmap.org/search?q=amsterdam&format=json&limit=1
```

Expected:
```json
[{"lat":"52.3676...","lon":"4.9041...",...}]
```

**Test Overpass (Restaurant Search):**

Go to: https://overpass-turbo.eu/

Paste and run:
```
[out:json][bbox:52.34,4.76,52.41,4.93];
(
  node[amenity=restaurant];
  way[amenity=restaurant];
  relation[amenity=restaurant];
);
out center 20;
```

Expected: Map with restaurant pins + JSON data

---

## ?? Test Results Tracker

Fill this in and share results:

```
Machine: Windows/Mac/Linux
.NET Version: 10.0
City Tested: Amsterdam
Message Sent: "restaurants in amsterdam"

Logs Show:
[ ] Extracting location
[ ] Geocoding to coordinates
[ ] Querying Overpass
[ ] Found X restaurants

Browser Shows:
[ ] Welcome message
[ ] Restaurant list
[ ] Google Maps links
[ ] Error message

Error (if any):
```

---

## ?? If Still No Results

### Check 1: Are Both Apps Running?
```bash
# Terminal 1:
cd RestaurantAi.Api && dotnet run
# Should show: Application started on https://localhost:7179

# Terminal 2:
cd RestaurantAi.Mvc && dotnet run
# Should show: Application started on https://localhost:7227
```

### Check 2: Rebuild Everything
```bash
dotnet clean
dotnet build
```

### Check 3: Check API Response Status
1. Open browser Developer Tools (F12)
2. Go to Network tab
3. Type in search
4. Check POST to `/api/AiChat/message`
5. Look at Response tab
6. Should show restaurant list or error message

### Check 4: Verify Services are Reachable

**Nominatim:**
```bash
curl -I https://nominatim.openstreetmap.org/search?q=test&format=json
# Should return 200 OK (not 503, 429, etc.)
```

**Overpass:**
```bash
curl -I https://overpass-api.de/api/interpreter
# Should return 200 OK
```

---

## ? Working Combination

If you're still stuck, try this exact setup:

1. **Close everything**
2. **Delete bin/obj folders**
   ```bash
   cd RestaurantAi.Api && rmdir /s /q bin obj
   cd RestaurantAi.Mvc && rmdir /s /q bin obj
   ```
3. **Rebuild**
   ```bash
   dotnet clean && dotnet build
   ```
4. **Start API**
   ```bash
   cd RestaurantAi.Api
   dotnet run
   ```
5. **In another terminal, start MVC**
   ```bash
   cd RestaurantAi.Mvc
   dotnet run
   ```
6. **Open dashboard**
   ```
   https://localhost:7227/Home/Dashboard
   ```
7. **Try: "restaurants in amsterdam"**

---

## ?? Specific Test Cases

### Test 1: Location Extraction
**Type:** `where can i find restaurants in london`
**Expected:** Extracts "london" and searches London

**Check logs for:** "Found coordinates for london"

### Test 2: Coordinate Search
**Type:** `restaurants near me` (with location enabled)
**Expected:** Uses your GPS coordinates

**Check logs for:** "Searching restaurants near coordinates:"

### Test 3: Default Fallback
**Type:** `show me restaurants`
**Expected:** Searches Amsterdam (default)

**Check logs for:** "Using default location: Amsterdam"

### Test 4: Multiple Results
**Type:** `italian restaurants in paris`
**Expected:** 5 restaurants in Paris

**Check logs for:** "Found 5 restaurants"

---

## ?? Debug Checklist

Before asking for help, verify:

- [ ] Both servers running (check terminals)
- [ ] Using major city (Amsterdam, London, Paris)
- [ ] No spelling errors in message
- [ ] API returning JSON (check Network tab in F12)
- [ ] Logs showing geocoding attempt
- [ ] Logs showing Overpass query
- [ ] Internet connection working (ping google.com)
- [ ] No 429 errors (rate limit) in logs
- [ ] Nominatim returning coordinates
- [ ] Overpass returning elements

---

## ?? Next Steps

**If restaurants ARE showing:** ?
- Great! System is working
- Try different cities to test
- Try "near me" if location enabled

**If restaurants are NOT showing:** ?
1. Share the **exact log output** from Visual Studio Output window
2. Share the **exact message** you typed
3. Share the **city** you searched for
4. Check the **API response** in browser Network tab
5. Paste all this information

---

## Common Quick Fixes

```
"Found 0 restaurants"
? Try: "restaurants in amsterdam"
? Check: Nominatim status
? Check: Overpass rate limit (try again later)

"Could not geocode location"
? Try: Different city name
? Check: City exists and is spelled right
? Try: Full name like "Amsterdam, Netherlands"

"Overpass API returned 429"
? Solution: Wait 5-10 minutes, try again
? This means: API is rate-limited (too many requests)

"Empty response from API"
? Check: Both servers are running
? Try: Refresh page
? Try: Ctrl+Shift+Delete to clear cache
```

---

**FASTEST FIX: Just try "restaurants in amsterdam" first!**

If that doesn't work, share your Visual Studio Output logs.
