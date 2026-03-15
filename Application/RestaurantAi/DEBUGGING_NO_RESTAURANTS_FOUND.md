# Restaurant Not Found - Debugging Guide

## ?? Why You're Getting "No Results"

The API is working, but the restaurant search is returning zero results. Here's how to debug it:

---

## Step 1: Check the API Logs

### In Visual Studio:
1. Open **Debug ? Windows ? Output**
2. Look for logs like:
   ```
   info: Searching restaurants by location: Amsterdam
   info: Found coordinates for Amsterdam: 52.37, 4.89
   info: Overpass returned 5 elements
   info: Found 5 restaurants for location Amsterdam
   ```

### What to look for:
```
? "Searching restaurants by location: [city]"
   ? Location extraction is working

? "Found coordinates for [city]"
   ? Nominatim geocoding succeeded

? "Overpass returned X elements"
   ? Overpass API is returning data

? "No elements property in Overpass response"
   ? Overpass returned empty or invalid response

? "Could not geocode location"
   ? Nominatim failed (wrong city name?)
```

---

## Step 2: Test Nominatim Directly

Open in your browser:
```
https://nominatim.openstreetmap.org/search?q=Amsterdam&format=json&limit=1
```

You should see:
```json
[
  {
    "lat": "52.3676...",
    "lon": "4.9041...",
    ...
  }
]
```

**If you get 429 error:**
- Nominatim is rate-limited
- Wait a few minutes and try again
- Add delay between requests

**If you get empty array []:**
- City name might be wrong
- Try a more specific location

---

## Step 3: Test Overpass Directly

Go to: https://overpass-turbo.eu/

Paste this query:
```
[out:json][bbox:52.3302,4.7639,52.4170,4.9346];
(
  node[amenity=restaurant];
  way[amenity=restaurant];
  relation[amenity=restaurant];
);
out center 20;
```

Click "Run" to test.

**What you should see:**
- Map with restaurant pins
- JSON response with restaurant data

**If you get "Too Many Requests":**
- Overpass API is busy
- Try again later
- This is normal during peak times

**If you get no results:**
- The data might not exist for that area
- Try a more central location
- Try a larger bbox

---

## Step 4: Common Issues & Solutions

### Issue 1: City Not Found
```
Message: "restaurants in Xyzville"
Log: "Could not geocode location: Xyzville"
```

**Solutions:**
- Use a major city (Amsterdam, London, Paris)
- Check spelling
- Try "restaurants near me" instead
- Try a more specific location (e.g., "Amsterdam, Netherlands")

### Issue 2: Zero Restaurants Found
```
Message: "restaurants in Amsterdam"
Log: "Overpass returned 0 elements"
```

**Possible causes:**
- Overpass API is rate-limited (try again later)
- The bbox coordinates are wrong
- No restaurants in OpenStreetMap for that area

**Solutions:**
1. Clear browser cache and retry
2. Try a different city
3. Wait 5-10 minutes and try again
4. Check if Overpass is down:
   ```
   https://status.overpass-api.de/
   ```

### Issue 3: Network Error
```
Log: "Overpass API returned 429"
   or "Nominatim geocoding failed"
```

**Causes:**
- External API is down/rate-limited
- Internet connection issue
- Firewall blocking requests

**Solutions:**
1. Try again after a few minutes
2. Check https://status.overpass-api.de/
3. Try from a different network
4. Check firewall/proxy settings

### Issue 4: Geolocation Not Working
```
Message: "restaurants near me"
Log: No coordinates logged
```

**Cause:**
- Geolocation permission denied
- Or timeout waiting for permission

**Solution:**
1. Click ?? lock in address bar
2. Reset location permission
3. Reload page
4. Click "Allow" when prompted

---

## Step 5: Manual Testing

### Test Case 1: Amsterdam
```
Message: "restaurants in amsterdam"
Expected: 5 restaurants in Amsterdam
```

### Test Case 2: London  
```
Message: "restaurants in london"
Expected: 5 restaurants in London
```

### Test Case 3: Paris
```
Message: "restaurants in paris"
Expected: 5 restaurants in Paris
```

### Test Case 4: Default
```
Message: "restaurants"
Expected: 5 restaurants in Amsterdam (default)
```

If any of these fail, note the city name and check logs.

---

## Step 6: Enable Debug Logging

Edit `RestaurantAi.Api/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);  // ? Add this
```

This will show all `_logger.LogDebug()` messages.

---

## Step 7: Check Internet Connectivity

These external services need internet access:

```bash
# Test Nominatim
curl "https://nominatim.openstreetmap.org/search?q=Amsterdam&format=json&limit=1"

# Test Overpass  
curl -X POST https://overpass-api.de/api/interpreter \
  -d "[bbox:52.3302,4.7639,52.4170,4.9346]; (node[amenity=restaurant]; way[amenity=restaurant]; relation[amenity=restaurant];); out center 20;"
```

If these fail, check your internet/firewall.

---

## Common Log Messages & What They Mean

```
? "Searching restaurants by location: Amsterdam"
   ? Location extraction working

? "Found coordinates for Amsterdam: 52.37, 4.89"
   ? Nominatim geocoding working

? "Overpass returned 8 elements"
   ? Overpass API working, found 8 restaurants

? "Found 5 restaurants for location Amsterdam"
   ? SUCCESS! Returning results to user

? "Could not geocode location: InvalidCity"
   ? Nominatim failed - city not found

? "Overpass API returned 429"
   ? Rate limited - try again later

? "Overpass API returned 500"
   ? Overpass server error - try again later

? "No elements property in Overpass response"
   ? Invalid response from Overpass - API might be down

? "Found 0 restaurants for location"
   ? No restaurants in OpenStreetMap for that area
```

---

## Quick Checklist

- [ ] Both applications are running (7179 + 7227)
- [ ] API is responding (check browser Network tab)
- [ ] Location is being extracted correctly (check logs)
- [ ] Nominatim is returning coordinates (check logs)
- [ ] Overpass is returning elements (check logs)
- [ ] Internet connection is working
- [ ] Using major city names (Amsterdam, London, Paris, etc.)
- [ ] Not hitting rate limits (wait between requests)

---

## Still Not Working?

1. **Post the API logs here** - Most helpful
2. **Check browser Network tab** - See what API returns
3. **Test Nominatim directly** - Via browser/curl
4. **Test Overpass directly** - Via https://overpass-turbo.eu/
5. **Check Overpass status** - https://status.overpass-api.de/

---

## Rate Limits

**Nominatim:**
- Max 1 request/second
- Recommended: Wait 1 second between searches

**Overpass:**
- Shared resource (global limit)
- If you get 429, wait 5-10 minutes
- Very busy times (late evening Europe) might fail

**Solution:**
- If searching multiple times, add delay
- Try again if you get rate limit errors

---

## Alternative Solutions

If Overpass is consistently unavailable:

1. **Use Google Places API** (requires API key)
2. **Use Yelp API** (requires API key)  
3. **Implement your own restaurant database** (complex)
4. **Use different geocoding** (e.g., Google Geocoding API)

---

## Contact Support

If you've tried all above steps:

1. Check API console output (F12 ? Console tab)
2. Check API logs in Visual Studio Output window
3. Note the exact error message
4. Note the city you searched for
5. Report with logs + error message

---

**Most Common Fix:**
```
1. Open Output window (Debug ? Windows ? Output)
2. Look for error messages
3. Usually it's one of:
   - Rate limiting (wait 5-10 min)
   - City not found (try different name)
   - API timeout (internet issue)
   - Wrong bbox format (unlikely after fix)
```

Try searching for "Amsterdam" first - it should always work!
