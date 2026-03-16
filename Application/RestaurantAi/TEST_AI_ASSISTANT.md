# ?? Quick Test - AI Assistant with Restaurants

## 2-Minute Setup

### Step 1: Restart API
```bash
cd RestaurantAi.Api
dotnet run
# Wait for: "Application started"
```

### Step 2: Open Dashboard
```
https://localhost:7227/Home/Dashboard
```

### Step 3: Test Restaurant Search
```
Type: restaurants near me
Expected: 5 restaurants from Oudenaarde area
Time: 2-3 seconds
```

### Step 4: Test General Questions
```
Type: What is the capital of France?
Expected: "The capital of France is Paris..."
Time: 1-2 seconds
```

### Step 5: Test Mixed Query
```
Type: Tell me about Amsterdam
Expected: Information about Amsterdam
Time: 1-2 seconds
```

## Test Cases

### Test 1: Restaurant Search ?
```
Message: "Pizza near me"
Expected: List of pizzerias
Shows: Restaurant list with maps
```

### Test 2: General Question ?
```
Message: "What is the weather like?"
Expected: Weather information
Shows: AI answer about weather
```

### Test 3: Calculate ?
```
Message: "What is 15% of 200?"
Expected: 30
Shows: Correct calculation
```

### Test 4: Definition ?
```
Message: "What is photosynthesis?"
Expected: Explanation of photosynthesis
Shows: Detailed explanation
```

### Test 5: Location Search ?
```
Message: "Restaurants in Brussels"
Expected: Brussels restaurants
Shows: Restaurant list with maps
```

## Expected Behavior

### When Restaurants Are Found
```
Here are some restaurants I found:

1. Restaurant Name • Location
   https://maps.google.com/...

2. Restaurant Name • Location
   ...

Would you like more details or to make a reservation?
```

### When Questions Are Asked
```
The capital of France is Paris. It's located in the 
north-central part of the country and is known for its 
iconic landmarks such as the Eiffel Tower...
```

## Success Indicators

? Both features work
? Restaurant lists appear for restaurant queries
? AI answers appear for general questions
? Responses within 2-4 seconds
? Language selector works
? Google Maps links functional

## Troubleshooting

**Issue:** No restaurant results
- **Check:** Location permission enabled
- **Try:** "restaurants in ghent" (city search)

**Issue:** AI not responding
- **Check:** Groq API key configured
- **Check:** API terminal for errors

**Issue:** Wrong language
- **Fix:** Use language selector (top-right)
- **Or:** Type in your preferred language

## Files to Check

- `RestaurantAi.Api/Controllers/AiChatController.cs` - Main logic
- `RestaurantAi.Mvc/Views/Home/Dashboard.cshtml` - Frontend
- `appsettings.json` - Configuration

## Command to Check Logs

**In API terminal, look for:**
```
info: RestaurantAi.Api.Controllers.AiChatController
      ? Found 5 restaurants with 5km radius
```

**Or for AI:**
```
Groq API call successful
Response: [AI answer]
```

---

## Summary

Your system now:
- ??? Finds restaurants
- ?? Answers questions
- ?? Looks things up
- ?? Supports multiple languages

**All powered by free APIs!** ?
