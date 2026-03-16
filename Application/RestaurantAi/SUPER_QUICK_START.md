# ?? QUICK REFERENCE - RUN YOUR APP NOW!

## ? 3-Step Setup (Do Once)

```powershell
# 1. Open PowerShell as Administrator
# 2. Copy-paste this:
cd "C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api"
dotnet user-secrets set "Groq:ApiKey" "YOUR_GROQ_API_KEY_HERE"

# 3. Verify it worked:
dotnet user-secrets list
```

**Should show:**
```
Groq:ApiKey = YOUR_GROQ_API_KEY_HERE
```

---

## ? Start App (Every Time)

### Open 2 Terminals Side-by-Side

**Terminal 1 (LEFT):**
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet run
```

**Terminal 2 (RIGHT):**
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Mvc
dotnet run
```

### Open Browser

```
https://localhost:7227/Home/Dashboard
```

---

## ? Test Commands

```
"Restaurants in Amsterdam"     ? Shows 5 restaurants
"More details about [name]"    ? Full info (hours, price, dishes)
"Restaurants near me"          ? Uses your location
```

---

## ? Security

```
? No API keys in code files
? No API keys in config files
? Keys only in User Secrets (secure)
? Safe to commit to Git
```

---

## ?? Ports

```
API:     https://localhost:7179
MVC:     https://localhost:7227
Database: restaurantAi.db (SQLite)
```

---

## ?? Done!

Just copy the PowerShell command above and run it once. Then you can start the app anytime! ??
