# ACTION PLAN: Fix Restaurant Search Now

## Right Now (Next 5 Minutes)

### Step 1: Restart Everything
```
1. Stop API (Ctrl+C in terminal)
2. Stop MVC (Ctrl+C in terminal)
3. Wait 5 seconds
4. Restart API:   cd RestaurantAi.Api && dotnet run
5. Restart MVC:   cd RestaurantAi.Mvc && dotnet run
6. Wait for both to show "Application started" messages
```

### Step 2: Clear Browser Cache
```
Chrome/Edge: Ctrl+Shift+Delete
Firefox: Ctrl+Shift+Delete  
Safari: Cmd+Y (then clear history)

Select: "Cookies and other site data"
Click: Clear data
```

### Step 3: Try Amsterdam
```
1. Open: https://localhost:7227/Home/Dashboard
2. Type: restaurants in amsterdam
3. Click: Send

Expected: List of 5 restaurants in 2-3 seconds
```

### Step 4: Check Logs
```
Visual Studio:
1. Click: Debug ? Windows ? Output
2. Look for these messages:
   ? "Searching restaurants by location: amsterdam"
   ? "Found coordinates for amsterdam: 52.37, 4.89"
   ? "Overpass returned X elements"
   ? "Found 5 restaurants for location amsterdam"
```

**If you see all 4 messages:** ? SUCCESS! System is working!

**If you see less than 4:** ? Note which message is missing and continue below.

---

## If That Doesn't Work (Next 10 Minutes)

### Scenario A: Logs Say "Could not geocode location"

**This means:** Nominatim isn't working

**Quick Fix:**
1. Wait 5 minutes (Nominatim might be rate-limited)
2. Try again with exact message: `restaurants in amsterdam`
3. If still fails, try: `restaurants in london`

**Verify Nominatim:**
```
Open in browser:
https://nominatim.openstreetmap.org/search?q=amsterdam&format=json&limit=1

You should see JSON with lat/lon coordinates
```

---

### Scenario B: Logs Say "Overpass returned 0 elements"

**This means:** Nominatim worked, but Overpass found no restaurants

**Quick Fix:**
1. Try different city: `restaurants in london`
2. Wait 5-10 minutes (Overpass might be rate-limited)
3. Try simpler message: `restaurants`

**Verify Overpass:**
```
Go to: https://overpass-turbo.eu/
Paste this query:
[out:json][bbox:52.34,4.76,52.41,4.93];
(
  node[amenity=restaurant];
  way[amenity=restaurant];
  relation[amenity=restaurant];
);
out center 20;

Click: Run
You should see restaurant pins on map
```

---

### Scenario C: API Returns Error (500 status)

**This means:** Something crashed in the API

**Quick Fix:**
1. Check Visual Studio Output for exception
2. Restart API: Stop (Ctrl+C) and `dotnet run` again
3. Try again

**If error persists:**
1. Rebuild: `dotnet clean && dotnet build`
2. Check for typos in code
3. Share the full exception message

---

### Scenario D: Network Error / No Response

**This means:** API isn't responding at all

**Quick Fix:**
1. Verify API is running
   - Terminal should show: "Application started on https://localhost:7179"
   - Check no red error messages
   
2. Verify ports are correct
   - API logs should show: "localhost:7179"
   - MVC logs should show: "localhost:7227"
   
3. Restart both applications
   - Stop API (Ctrl+C)
   - Stop MVC (Ctrl+C)
   - Start API again
   - Start MVC again

---

## Definitive Test (If Above Doesn't Work)

### Use cURL to Test API Directly

**Option 1: Windows PowerShell**
```powershell
$body = @{
    Message = "restaurants in amsterdam"
    SessionId = ""
    Language = "en"
    Latitude = $null
    Longitude = $null
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:7179/api/AiChat/message" `
  -Method POST `
  -Headers @{"Content-Type"="application/json"} `
  -Body $body `
  -SkipCertificateCheck
```

**Option 2: Git Bash / Command Line**
```bash
curl -X POST https://localhost:7179/api/AiChat/message \
  -H "Content-Type: application/json" \
  -d "{\"Message\":\"restaurants in amsterdam\",\"SessionId\":\"\",\"Language\":\"en\",\"Latitude\":null,\"Longitude\":null}"
```

**Expected Response:**
```json
{
  "reply": "Here are some restaurants I found:\n\n1...",
  "sessionId": "..."
}
```

**What different responses mean:**
```
? Long JSON with "Here are some restaurants"
   ? API and search working!

? Long JSON with "I couldn't find restaurants"
   ? API works, but Overpass/Nominatim issue

? Empty response or nothing
   ? API not responding (check if running)

? "Connection refused"
   ? API not running on 7179 (check terminal)

? "Untrusted certificate" warning
   ? Normal for localhost HTTPS, ignore
```

---

## If CURL Test Works But Browser Doesn't

**Issue:** CORS (cross-origin) problem

**Fix:**
1. Check MVC port is in CORS config
2. Open `RestaurantAi.Api/Program.cs`
3. Look for `AddCors` section
4. Make sure it includes: `https://localhost:7227`
5. Rebuild and restart

---

## Nuclear Option (If Everything Else Fails)

```bash
# Delete all caches
cd RestaurantAi.Api
rmdir /s /q bin obj
cd ../RestaurantAi.Mvc
rmdir /s /q bin obj
cd ..

# Clean rebuild
dotnet clean
dotnet build

# Restart both with full output
cd RestaurantAi.Api
dotnet run --verbose

# In another terminal:
cd RestaurantAi.Mvc
dotnet run --verbose

# Test: https://localhost:7227/Home/Dashboard
# Type: restaurants in amsterdam
```

---

## Decision Tree

```
Does restaurant search work?
?? YES ?
?  ?? Great! You're done. Enjoy!
?
?? NO ?
   ?
   ?? Is API running?
   ?  ?? NO ? Start: cd RestaurantAi.Api && dotnet run
   ?  ?
   ?  ?? YES ? Continue
   ?
   ?? Is MVC running?
   ?  ?? NO ? Start: cd RestaurantAi.Mvc && dotnet run
   ?  ?
   ?  ?? YES ? Continue
   ?
   ?? Do you see API logs?
   ?  ?? NO ? Check "Application started on https://localhost:7179"
   ?  ?
   ?  ?? YES ? Continue
   ?
   ?? Do logs show "Searching restaurants by location"?
   ?  ?? NO ? Location extraction failing, try "restaurants in amsterdam"
   ?  ?
   ?  ?? YES ? Continue
   ?
   ?? Do logs show "Found coordinates"?
   ?  ?? NO ? Nominatim failing, wait 5 min (rate limit) and retry
   ?  ?
   ?  ?? YES ? Continue
   ?
   ?? Do logs show "Overpass returned X elements"?
   ?  ?? NO ? Overpass not responding, check status.overpass-api.de
   ?  ?
   ?  ?? YES ? Continue
   ?
   ?? Do logs show "Found 5 restaurants"?
      ?? NO ? Check screenshot/full logs
      ?
      ?? YES ? SUCCESS! ?
         (If browser not showing, it's a UI issue)
```

---

## What to Share If Stuck

**Please provide:**

1. **Exact message you typed:**
   ```
   e.g., "restaurants in amsterdam"
   ```

2. **Complete log output from Visual Studio:**
   ```
   (Ctrl+A in Output window, paste entire thing)
   ```

3. **API response status:**
   ```
   Open F12 ? Network ? See POST response
   ```

4. **City you searched:**
   ```
   e.g., Amsterdam, London, Paris
   ```

5. **Error message in browser (if any):**
   ```
   Screenshot of error
   ```

---

## Expected Timeline

| Step | Time | What You're Doing |
|------|------|-------------------|
| 1 | 1 min | Restart both apps |
| 2 | 30 sec | Clear browser cache |
| 3 | 30 sec | Try "restaurants in amsterdam" |
| 4 | 1 min | Check API logs |
| 5 | 2 min | Verify Nominatim/Overpass if needed |

**Total: ~5 minutes to success or diagnosis**

---

## Success Checklist

- [ ] Both API and MVC are running
- [ ] You typed a message with a city
- [ ] API logs show "Searching restaurants by location"
- [ ] API logs show "Found coordinates"
- [ ] API logs show "Overpass returned X elements"
- [ ] Browser shows list of restaurants
- [ ] Each restaurant has Google Maps link

**If all checked:** ?? You're done! System is working!

---

## Still Stuck?

Gather this info and ask for help:

1. Which step above fails?
2. Complete API log output
3. What city you tried
4. Screenshot of error (if any)
5. Are you seeing "Found coordinates" in logs?
6. Are you seeing "Overpass returned" in logs?

**Most likely causes:**
- Nominatim rate-limited (wait 5 min)
- Overpass rate-limited (wait 10 min)  
- Wrong city name (try "Amsterdam")
- Apps not running (check terminals)
- Cache not cleared (Ctrl+Shift+Delete)
