# ?? Diagnostic: Check Why Restaurants Aren't Found

## The Problem
```
? Location: Oudenaarde (found correctly)
? Coordinates: Obtained from IP address
? Restaurants: Not found
```

## What to Check

### Step 1: Look at API Logs

When you search "restaurants near me", check the API output for these messages:

**Expected sequence:**
```
=== RESTAURANT SEARCH DEBUG ===
Location: null, Lat: 50.94, Lng: 3.61
Using GPS coordinates: 50.94, 3.61
Executing Overpass query for 50.94,3.61
Overpass returned 0 elements
```

**Or if it works:**
```
? Found 5 restaurants with 5km radius
```

### Step 2: Find These Specific Messages

**Look for:**
- `Using GPS coordinates:` - Should show your coordinates
- `Overpass returned X elements` - Should be > 0
- `? Found X restaurants` - Success!

**Or error messages:**
- `? No restaurants found with Overpass even with 10km radius`
- `Reverse geocoded to city:` - Trying fallback method

### Step 3: Check API Response in Browser

1. Open **Developer Tools** (F12)
2. Go to **Network** tab
3. Send message: `restaurants near me`
4. Find POST to `/api/AiChat/message`
5. Check **Response** tab

**Expected response:**
```json
{
  "reply": "Here are some restaurants I found:\n1. Restaurant Name...",
  "sessionId": "..."
}
```

**Or error response:**
```json
{
  "reply": "I couldn't find restaurants for that location...",
  "sessionId": "..."
}
```

## Why It Might Fail

### Reason 1: Coordinates Not Passed
**Symptom:** API logs show `Lat: null, Lng: null`

**Solution:**
- Check browser console for geolocation message
- Should show: `? IP-based location obtained`
- If not, geolocation isn't working

### Reason 2: Overpass Has No Data
**Symptom:** API logs show `Overpass returned 0 elements`

**Solution:**
- This is normal for very small towns
- System should try reverse geocoding
- Then search by city name

### Reason 3: Oudenaarde Has Limited OpenStreetMap Data
**Symptom:** Logs show it tried 5km and 10km, both return 0

**Solution:**
- Try: `"restaurants in ghent"` (nearby city)
- Or: `"restaurants in bruges"`
- Larger cities have better data

## How the Fallback Works

```
User: "restaurants near me"
  ?
Get coordinates: 50.94, 3.61 (Oudenaarde)
  ?
Search 5km ? 0 restaurants found
  ?
Search 10km ? 0 restaurants found
  ?
Reverse geocode: Get city name "Oudenaarde"
  ?
Search restaurants "in Oudenaarde"
  ?
Get city coordinates and try again
  ?
Search 5km around Oudenaarde city center
  ?
If still nothing: Search by name globally
```

## Testing the Fallback

### Test 1: Check if Geolocation Works
```
Open DevTools Console
Should see: "? IP-based location obtained: {latitude: 50.94, longitude: 3.61}"
```

### Test 2: Check if Reverse Geocoding Works
```
API logs should show: "Reverse geocoded to city: Oudenaarde"
```

### Test 3: Check if City Search Works
```
Type: "restaurants in oudenaarde"
Should work or show "couldn't find"
```

### Test 4: Check with Major City
```
Type: "restaurants in ghent"
Expected: Should find restaurants
```

## Debug Checklist

- [ ] Geolocation message in console shows coordinates
- [ ] API logs show coordinates being used
- [ ] API logs show Overpass query attempt
- [ ] API logs show element count returned
- [ ] Browser Network tab shows API response
- [ ] Response contains restaurant list or error message

## Common Issues & Fixes

| Issue | Symptom | Fix |
|-------|---------|-----|
| No coordinates | Lat/Lng null in logs | Check geolocation in console |
| No OpenStreetMap data | Overpass returns 0 | Try nearby city name |
| API timeout | Search takes >15 sec | Wait, Overpass might be slow |
| CORS error | Network error in console | Check API CORS config |

## If Still Not Working

1. **Restart API:**
   ```bash
   cd RestaurantAi.Api
   dotnet run
   ```

2. **Check API logs in terminal**
   - Look for error messages
   - Note the exact sequence of events
   - Share the logs if stuck

3. **Try different city:**
   - "restaurants in ghent"
   - "restaurants in bruges"
   - "restaurants in antwerp"

4. **Check Overpass status:**
   - https://status.overpass-api.de/
   - API might be down

## Quick Diagnostic Command

**In browser console, run:**
```javascript
// Check if geolocation worked
console.log('Current coords:', currentCoords);

// This should show your location from IP
// Example: {latitude: 50.94, longitude: 3.61}
```

## Next Steps

Once you run "restaurants near me", **share these details:**

1. **Browser console message** (the `? IP-based location obtained` line)
2. **API log output** (all lines with "RESTAURANT SEARCH DEBUG")
3. **What you typed** (the exact message)
4. **Error or success** (did you get restaurants?)

This will help diagnose exactly where the issue is!

---

**Current Status:** ? System is designed to handle Oudenaarde. Debugging improvements are now in place to show exactly what's happening.
