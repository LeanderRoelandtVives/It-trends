# ?? Test Restaurant Details Feature - 5 Minutes

## Quick Test

### Step 1: Start API (30 seconds)
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet run
```
Wait for: "Application started"

### Step 2: Open Dashboard (10 seconds)
```
https://localhost:7227/Home/Dashboard
```

### Step 3: Test Scenario A - Simple Details Request

**Message 1:**
```
Type: "Restaurants in Oudenaarde"
Expected: 5 restaurants listed
Wait: 2-3 seconds
```

**Message 2:**
```
Type: "Tell me more details"
Expected: AI provides information about restaurants, 
          dining options, what to expect, etc.
Wait: 1-2 seconds
```

### Step 4: Test Scenario B - Ask Specific Questions

**Message 1:**
```
Type: "Pizza near me"
Expected: 5 nearby pizzerias
```

**Message 2:**
```
Type: "What about Italian cuisine?"
Expected: Information about Italian food, traditions, 
          dining culture, what to expect
```

### Step 5: Test Scenario C - More Info

**Message 1:**
```
Type: "Restaurants in Brussels"
Expected: Restaurant list
```

**Message 2:**
```
Type: "More info about these restaurants"
Expected: Details about Brussels dining scene, 
          recommendations, what to expect
```

## Expected Responses

### For "Tell me more details"
```
Italian restaurants typically feature:
- Traditional dishes like pasta, risotto, and seafood
- Quality ingredients and fresh preparations
- Wine pairings with meals
- Warm, welcoming atmosphere
- Family dining culture

Tips when visiting:
- Allow time for leisurely meals
- Make reservations for popular spots
- Dress appropriately for the venue
- Budget ranges from casual to fine dining
- Service is typically attentive
```

### For "What about [cuisine]?"
```
Belgian cuisine is known for:
- Waffles (both sweet and savory)
- Belgian fries and sauces
- Beer selection and culture
- Seafood specialties
- Chocolate

When dining in Belgium:
- Meals are typically leisurely
- Beer is integral to the culture
- Quality ingredients are paramount
- Restaurant culture is highly valued
```

## Success Criteria

? **Restaurant search works**
- See list of 5 restaurants
- Shows restaurant names and maps

? **Detail query works**
- System recognizes "tell me more"
- AI provides relevant information
- Takes 1-2 seconds

? **Contextual answers**
- Responses relate to restaurants/dining
- Appropriate for the language (EN/NL)
- Helpful and informative

? **No errors**
- Check browser console (F12)
- Check API terminal for errors
- Responses are complete

## Troubleshooting

**Issue:** "AI service is not configured"
- **Fix:** Check Groq API key in user secrets
- Run: `dotnet user-secrets list`
- Should show `Groq:ApiKey`

**Issue:** "I couldn't retrieve the details"
- **Fix:** Groq API might be rate-limited
- **Solution:** Wait 1 minute and try again

**Issue:** Slow responses (> 3 seconds)
- **Cause:** Groq API is busy
- **Solution:** This is normal, try again

**Issue:** No error but no response
- **Fix:** Check browser console (F12)
- Check API terminal logs
- Restart API if needed

## Test Messages to Try

### Restaurant Search + Details

```
"Restaurants in Amsterdam"
? "Tell me more details"

"Pizza near me"
? "More info about Italian cuisine"

"Food in Brussels"
? "What should I know about Belgian restaurants?"

"Restaurants in Paris"
? "Details about French dining"
```

### Follow-up Questions

```
"More information about bistros"
"Details about budget dining"
"What's the atmosphere like?"
"Any tips for reservations?"
"Tell me about the cuisine"
```

## API Logs to Watch

**In API terminal, you should see:**

```
Stored 5 restaurants in cache for session [ID]
Executing Groq query for detail request
Groq API call successful
Response retrieved with details
```

**Indicates success:** ?

## Browser DevTools Check

**Open:** F12 ? Console tab

**Should NOT see:**
- ? "AI service is not configured"
- ? "Failed to get restaurant details"
- ? Network errors

**Should see:**
- ? Response text appears in chat
- ? No red error messages
- ? Complete response

## Performance Expectations

| Operation | Time |
|-----------|------|
| Restaurant search | 2-3 sec |
| Detail query | 1-2 sec |
| "Tell me more" | 1-2 sec |
| Follow-up question | 1-2 sec |

## Multi-Language Test

### English Test
```
Message: "Restaurants in London"
Response: English restaurant list

Message: "Tell me more details"
Response: English details about dining in London
```

### Dutch Test
```
Message: "Restaurants in Amsterdam"  
Response: Dutch restaurant list (if language = nl)

Message: "Vertel me meer details"
Response: Dutch details about dining
```

## Success Path

1. ? Restaurants found in search
2. ? Detail query recognized
3. ? AI generates response
4. ? Response appears in chat
5. ? No errors in console

**If all 5 ?, feature is working!** ??

---

**Time required:** ~5 minutes
**Difficulty:** Easy
**Success rate:** Should be 100% if API key is configured
