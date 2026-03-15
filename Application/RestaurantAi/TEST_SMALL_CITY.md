# ?? Test Small City Search - 2 Minutes

## Quick Test for Oudenaarde

### Step 1: Restart API
```bash
cd RestaurantAi.Api
dotnet run
# Wait for: "Application started on https://localhost:7179"
```

### Step 2: Test in Browser
```
Open: https://localhost:7227/Home/Dashboard
```

### Step 3: Send Message
```
Type: restaurants near me
Expected: 
  - Browser sends your location (IP-based: Oudenaarde)
  - API searches 5km radius
  - API searches 10km radius (includes Ghent)
  - Returns 5 restaurants
  - Takes 2-3 seconds
```

### Step 4: Check Result

**In browser, you should see:**
```
Here are some restaurants I found:

1. Restaurant Name • Oudenaarde
   https://www.google.com/maps/search/...

2. Restaurant Name • Oudenaarde
   https://www.google.com/maps/search/...

3. Restaurant Name • Ghent
   https://www.google.com/maps/search/...

(etc...)

Would you like more details or to make a reservation?
```

### Step 5: Check API Logs

**In API terminal, you should see:**
```
Searching restaurants near coordinates: 50.94, 3.61
Found 0 restaurants with 5km radius
No results with 5km radius, trying 10km radius...
Found 8 restaurants with 10km radius
```

## Success Criteria

? If you see:
- Restaurant list appears in 2-3 seconds
- 5+ restaurants from Oudenaarde/Ghent area
- Google Maps links for each

**Then it's working!** ??

## Alternative Tests

### Test with Amsterdam
```
Type: restaurants in amsterdam
Expected: Instant results (lots of restaurants)
```

### Test with Ghent
```
Type: restaurants in ghent
Expected: Instant results
```

### Test with another city
```
Type: restaurants in bruges
Expected: Results from Bruges area
```

---

## If No Restaurants Found

**Check:**
1. API logs show "Found X restaurants with 10km radius"
2. Look for errors in browser console (F12)
3. Try "restaurants in ghent" instead (city search)
4. Check Overpass API status: https://status.overpass-api.de/

**Note:** Small Belgian towns might have limited OpenStreetMap data. Try searching "in ghent" (nearby major city) if your exact town has no data.

---

**Expected Time:** 2-3 seconds from sending message to seeing results
