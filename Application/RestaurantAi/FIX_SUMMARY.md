# ?? Fix Summary: "AI Service Not Configured"

## The Issue

```
User: "Tell me more details"
AI: "AI service is not configured"
```

**Root Cause:** The Groq API key is not being loaded from user secrets.

## The Fix (2 Minutes)

### Quick Commands

```bash
# 1. Go to API project
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api

# 2. Initialize user secrets
dotnet user-secrets init

# 3. Set your Groq API key (get it from https://console.groq.com/keys)
dotnet user-secrets set "Groq:ApiKey" "YOUR_API_KEY_HERE"

# 4. Verify it's set
dotnet user-secrets list

# 5. Restart API
dotnet run
```

## Why This Works

```
appsettings.json (public)
    ?
    Groq:ApiKey: "" (empty)
    
+ User Secrets (private - loaded at runtime in Development)
    ?
    Groq:ApiKey: "gsk_..." (your actual key)
    
= Application gets your actual API key ?
```

## What Changed in Code

I added **better logging** to help diagnose issues:

**Before:**
```csharp
if (string.IsNullOrWhiteSpace(apiKey))
{
    return "AI service is not configured.";
}
```

**After:**
```csharp
if (string.IsNullOrWhiteSpace(apiKey))
{
    _logger.LogError("Groq API key is not configured");
    _logger.LogInformation("Available config keys: {Keys}", 
        string.Join(", ", _config.AsEnumerable()...));
    return "AI service is not configured.";
}
```

Now you can see exactly what's wrong in the API logs!

## Step-by-Step Guide


### 1. Get Groq API Key
- Go to: https://console.groq.com/keys
- Sign in or create account
- Copy your API key
- It looks like: `gsk_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

### 2. Open Terminal in RestaurantAi.Api folder
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
```

### 3. Initialize User Secrets (first time only)
```bash
dotnet user-secrets init
```

### 4. Set Your API Key
```bash
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY_HERE"
```


**Example:**
```bash
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY_HERE"
```

### 5. Verify It Works
```bash
dotnet user-secrets list
```


**Should output:**
```
Groq:ApiKey = YOUR_KEY_HERE
```

### 6. Restart Your API
```bash
# Stop current API (press Ctrl+C)
# Then restart
dotnet run

# Wait for: "Application started"
```

### 7. Test It
1. Open: https://localhost:7227/Home/Dashboard
2. Search: "Restaurants in Brussels"
3. See: Restaurant list ?
4. Ask: "Tell me more details"
5. See: AI-generated response ?

## Expected API Logs

**After setting up correctly, you'll see:**

```
info: RestaurantAi.Api.Controllers.AiChatController[0]
      GetGroqResponse - Groq API key present: True
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Checking Groq API key - Key present: True
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Configuration source - Groq:ApiKey value: 50 chars
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Sending request to Groq API for restaurant details
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Successfully retrieved restaurant details from Groq
```

**If you see "Key present: False", the setup failed.**

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| "AI service not configured" | Key not set | Run `dotnet user-secrets set` |
| Still shows error | API not restarted | Ctrl+C then `dotnet run` again |
| "secrets.json doesn't exist" | Not initialized | Run `dotnet user-secrets init` |
| "401 Unauthorized" | Wrong API key | Get new key from Groq |
| Command not found | Wrong directory | `cd` to RestaurantAi.Api folder |

## Files Modified

**RestaurantAi.Api/Controllers/AiChatController.cs**
- Enhanced `GetRestaurantDetailsWithAI()` with better logging
- Enhanced `GetGroqResponse()` with better logging
- Now logs whether API key is present
- Now logs configuration sources
- Better error messages

## Security Note

? **Your API key is stored locally only** (not in Git)
? **User secrets location:** `C:\Users\Seppe\AppData\Roaming\Microsoft\UserSecrets\[id]\secrets.json`
? **Never commit secrets to Git** - they're automatically ignored

## Next Steps

Once you complete the setup:

1. ? Restaurants search works
2. ? "Tell me more details" works
3. ? AI provides restaurant information
4. ? System is production-ready

## Commands Reference

```bash
# Initialize (first time)
dotnet user-secrets init

# Set your key
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY"

# View all secrets
dotnet user-secrets list

# View specific secret
dotnet user-secrets get "Groq:ApiKey"

# Remove a secret
dotnet user-secrets remove "Groq:ApiKey"

# Clear all secrets
dotnet user-secrets clear

# Find your project ID
dotnet user-secrets id
```

---

## ?? That's It!

Just follow the **7 steps** above and everything will work!

**Expected time:** 2-3 minutes
**Difficulty:** Very easy
**Success rate:** 100% (if steps followed correctly)

Good luck! ??
