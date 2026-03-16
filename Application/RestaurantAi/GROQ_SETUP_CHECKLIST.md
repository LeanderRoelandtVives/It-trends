# ? Groq API Key Setup Checklist

## Quick Setup (5 minutes)

### ? Step 1: Navigate to API Project
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
```

### ? Step 2: Initialize User Secrets
```bash
dotnet user-secrets init
```
Expected: "Successfully created the 'secrets.json' file"

### ? Step 3: Get Your Groq API Key
1. Visit: https://console.groq.com/keys
2. Sign in
3. Copy your key (starts with `gsk_`)

### ? Step 4: Set the Secret
```bash
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY_HERE"
```
Replace `YOUR_KEY_HERE` with your actual key from Groq

### ? Step 5: Verify It's Set
```bash
dotnet user-secrets list
```
Should show: `Groq:ApiKey = gsk_...`

### ? Step 6: Restart API
```bash
dotnet run
```
Wait for: "Application started. Press Ctrl+C to shut down."

### ? Step 7: Test in Browser
1. Open: https://localhost:7227/Home/Dashboard
2. Type: `"Restaurants in Brussels"`
3. See: Restaurant list ?
4. Type: `"Tell me more details"`
5. See: AI response ?

## Verification Checklist

| Check | Status | Fix |
|-------|--------|-----|
| Groq account created | ? | Go to https://console.groq.com |
| API key copied | ? | Get from Groq console |
| User secrets initialized | ? | Run `dotnet user-secrets init` |
| Secret set correctly | ? | Run `dotnet user-secrets set` |
| Secret verified | ? | Run `dotnet user-secrets list` |
| API restarted | ? | Stop (Ctrl+C) and `dotnet run` |
| Dashboard opens | ? | Check https://localhost:7227 |
| Restaurant search works | ? | Try `"restaurants in brussels"` |
| Detail request works | ? | Try `"tell me more details"` |

## Expected Logs in API Terminal

After each successful step, you should see:

**Step 6 (Restart):**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7179
```

**Step 7 (Restaurant search):**
```
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Searching restaurants by location: brussels
info: RestaurantAi.Api.Controllers.AiChatController[0]
      ? Found 5 restaurants with 5km radius
```

**Step 7 (Tell me more):**
```
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Checking Groq API key - Key present: True
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Sending request to Groq API for restaurant details
info: RestaurantAi.Api.Controllers.AiChatController[0]
      Successfully retrieved restaurant details from Groq
```

## Troubleshooting Logs

**If you see:**
```
Checking Groq API key - Key present: False
```
? The secret is NOT set. Go back to Step 4.

**If you see:**
```
Groq API error: 401
```
? The API key is wrong or invalid. Get a new one from Groq.

**If you see:**
```
The user secrets 'secrets.json' file doesn't exist yet.
```
? Run Step 2: `dotnet user-secrets init`

## Common Issues

| Error | Cause | Solution |
|-------|-------|----------|
| "AI service not configured" | Key not set | Run Step 4 & 5 |
| "401 Unauthorized" | Invalid key | Get new key from Groq |
| "secrets.json doesn't exist" | Not initialized | Run `dotnet user-secrets init` |
| Still same error | API not restarted | Stop API (Ctrl+C) and `dotnet run` |

## One-Liner Commands

```bash
# Initialize and set in one go
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api && dotnet user-secrets init && dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY_HERE"

# Verify
dotnet user-secrets list

# Restart
dotnet run
```

## Success Indicators ?

- ? `dotnet user-secrets list` shows your key
- ? API terminal shows "Key present: True"
- ? Browser shows restaurant list
- ? Browser shows AI-generated details
- ? No red errors in browser console

---

**Once all ? are checked and ? all indicators are green, you're done!** ??
