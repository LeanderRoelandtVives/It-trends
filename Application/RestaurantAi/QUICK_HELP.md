# ?? START HERE - Restaurant Search Troubleshooting

## ? 2-Minute Fix

```bash
# 1. Stop everything (Ctrl+C in both terminals)

# 2. Clear browser
# Ctrl+Shift+Delete ? Select All ? Clear Data

# 3. Rebuild
dotnet clean
dotnet build

# 4. Start API
cd RestaurantAi.Api
dotnet run
# Wait for: "Application started on https://localhost:7179"

# 5. In another terminal, start MVC
cd RestaurantAi.Mvc
dotnet run
# Wait for: "Application started on https://localhost:7227"

# 6. Test
# Open: https://localhost:7227/Home/Dashboard
# Type: restaurants in amsterdam
# Expected: 5 restaurants in ~2-3 seconds
```

---

## ?? Check API Logs

**Visual Studio:** Debug ? Windows ? Output

**Look for these messages:**
```
? "Searching restaurants by location: amsterdam"
? "Found coordinates for amsterdam: 52.37, 4.89"
? "Overpass returned 5 elements"
? "Found 5 restaurants for location amsterdam"
```

**If you see these 4:** ?? SUCCESS!

**If you see fewer:** Note which is missing and continue below.

---

## ?? If Still Not Working

### If logs say "Could not geocode location"
- Nominatim (geocoding service) is failing
- **Solution:** Wait 5 minutes and try again
- **Alternative:** Try different city: "restaurants in london"

### If logs say "Overpass returned 0 elements"
- Overpass API (restaurant database) found no data
- **Solution:** Wait 10 minutes and try again
- **Alternative:** Try different city
- **Check:** https://status.overpass-api.de/

### If you see no logs at all
- API might not be running
- **Check:** Terminal shows "Application started on https://localhost:7179"
- **Solution:** Stop and restart API

### If browser shows error
- **Clear cache:** Ctrl+Shift+Delete
- **Check:** Both terminals show "Application started"
- **Try:** Refresh page with Ctrl+F5

---

## ?? Test Results

| Message | Location | Logs Show | Result | Next Step |
|---------|----------|-----------|--------|-----------|
| "restaurants in amsterdam" | Amsterdam | 4 success messages | ? Works | Enjoy! |
| "restaurants in amsterdam" | Amsterdam | 3 success messages | ? Overpass issue | Wait 10 min |
| "restaurants in amsterdam" | Amsterdam | 2 success messages | ? Nominatim issue | Wait 5 min |
| "restaurants in amsterdam" | Amsterdam | 1 success message | ? Extraction issue | Try "restaurants in london" |
| "restaurants in amsterdam" | Amsterdam | 0 success messages | ? API not running | Check terminal |

---

## ?? Quick Checklist

- [ ] Both apps are running (check terminals)
- [ ] Browser cache is cleared
- [ ] You typed "restaurants in amsterdam"
- [ ] Output window is open (Debug ? Windows ? Output)
- [ ] You see "Searching restaurants by location" in logs
- [ ] You see "Found coordinates" in logs
- [ ] You see "Overpass returned" in logs
- [ ] Browser shows restaurant list

**Check all 8:** System works! ?

---

## ?? Most Likely Issue

**You're hitting rate limits on external APIs**

- Nominatim (geocoding): Max 1 request/second
- Overpass (restaurants): Shared global limit

**Quick fixes:**
1. Wait 5-10 minutes
2. Try again
3. If still fails, try different city

---

## ?? Getting Help

**Share this info:**
1. Exact message you typed
2. All log lines from Output window
3. City you searched for
4. Screenshot of browser error (if any)

**Most helpful:** Paste complete log output from "Searching restaurants" onwards

---

## ?? Detailed Guides

- **ACTION_PLAN.md** - Complete step-by-step fix
- **DEBUGGING_NO_RESTAURANTS_FOUND.md** - Detailed troubleshooting
- **FAST_TEST_GUIDE.md** - Quick tests
- **CURRENT_STATUS_AND_SOLUTION.md** - Full situation overview

---

## ? When It Works

You should see:
```
Here are some restaurants I found:

1. Restaurant Name • amsterdam
   https://www.google.com/maps/search/?api=1&query=...

2. Restaurant Name • amsterdam
   https://www.google.com/maps/search/?api=1&query=...

3. Restaurant Name • amsterdam
   ...

Would you like more details or to make a reservation?
```

Each restaurant name is a clickable Google Maps link!

---

## ?? Quick Video Summary

1. Restart both apps
2. Clear browser cache
3. Try "restaurants in amsterdam"
4. Check API logs for 4 success messages
5. If success: Done! If not: Wait 5-10 minutes and retry

---

## ?? Pro Tips

- Amsterdam always works (largest dataset)
- Try London if Amsterdam fails
- Wait between multiple searches
- Clear cache between tests
- Check terminal for error messages

---

## ?? Common Mistakes

? Typing "find me italian restaurants"
? Instead type: "italian restaurants in amsterdam"

? Searching 5 times in a row
? Wait 1 minute between searches

? Not restarting apps
? Restart with: Ctrl+C then `dotnet run`

? Not clearing cache
? Clear with: Ctrl+Shift+Delete ? All

---

## ?? Expected Times

- Restart apps: 30 seconds
- Clear cache: 10 seconds
- Send message: 1 second
- API response: 2-3 seconds
- Total: ~1 minute

**Total with retries: ~10-15 minutes max**

---

## ?? Success!

If you see restaurants with Google Maps links: ? **System works!**

Enjoy your restaurant search chatbot! ???

---

**Still stuck?** Share your logs + follow ACTION_PLAN.md
