# ?? QUICK START - SECURE (NO HARDCODED KEYS)

## ? Status

```
Build: ? Successful
Code: ? No hardcoded API keys
Secrets: ? User Secrets only
Ready: ? To use
```

---

## ?? One-Time Setup (Copy-Paste)

**PowerShell (Admin):**

```powershell
cd "C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api"

# Set Groq API key
dotnet user-secrets set "Groq:ApiKey" "YOUR_GROQ_API_KEY_HERE"

# Verify
dotnet user-secrets list
```

**Expected Output:**
```
Groq:ApiKey = YOUR_GROQ_API_KEY_HERE
```

---

## ?? Run (Every Time)

### Terminal 1
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet run
```

### Terminal 2 (NEW)
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Mvc
dotnet run
```

### Browser
```
https://localhost:7227/Home/Dashboard
```

---

## ?? Test

```
Type: "Restaurants in Amsterdam"
See: Restaurant list ?
```

---

## ?? Done!

All secure, no hardcoded keys! ???
