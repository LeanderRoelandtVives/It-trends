# ? RESTAURANT AI - SECURE SETUP (NO HARDCODED KEYS)

## ?? Security Configuration

All API keys are stored in **User Secrets**, NOT in code:

```
? Code: NO API keys hardcoded
? appsettings.json: NO API keys (empty strings)
? User Secrets: API keys stored securely
? Program.cs: Loads from user secrets in Development
```

---

## ?? Required User Secrets Setup

### Check if Already Set

**PowerShell (Run as Administrator):**
```powershell
cd "C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api"
dotnet user-secrets list
```

**Expected Output:**
```
Groq:ApiKey = [your key here]
Authentication:Google:ClientId = [your ID here]
Authentication:Google:ClientSecret = [your secret here]
```

---

## ?? Setup User Secrets (If Not Already Done)

### Step 1: Set Groq API Key

```powershell
cd "C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api"
dotnet user-secrets set "Groq:ApiKey" "YOUR_GROQ_API_KEY_HERE"
```

**Expected:**
```
Successfully saved Groq:ApiKey to the secret store.
```

### Step 2: Set Google OAuth (Optional, for authentication)

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID_HERE"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET_HERE"
```

---

## ? Verify Configuration

```powershell
dotnet user-secrets list
```

**You should see:**
```
Groq:ApiKey = YOUR_GROQ_API_KEY_HERE
Authentication:Google:ClientId = YOUR_GOOGLE_CLIENT_ID
Authentication:Google:ClientSecret = YOUR_GOOGLE_CLIENT_SECRET
```

---

## ?? Where Secrets Are Stored

User secrets are stored securely in:
```
Windows: %APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json
Mac/Linux: ~/.microsoft/usersecrets/{UserSecretsId}/secrets.json
```

**Your RestaurantAi.Api UserSecretsId:**
```
e66efa96-db2e-40e7-bef9-87fd8b3f3a19
```

**Actual Location:**
```
C:\Users\Seppe\AppData\Roaming\Microsoft\UserSecrets\e66efa96-db2e-40e7-bef9-87fd8b3f3a19\secrets.json
```

---

## ?? Start Your App

### Terminal 1: API

```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet run
```

**Wait for:**
```
Now listening on: https://localhost:7179
Application started. Press Ctrl+C to shut down.
```

### Terminal 2: MVC (NEW WINDOW)

```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Mvc
dotnet run
```

**Wait for:**
```
Now listening on: https://localhost:7227
Application started. Press Ctrl+C to shut down.
```

---

## ?? Open Dashboard

```
https://localhost:7227/Home/Dashboard
```

---

## ?? Test Commands

### Test 1: Basic Search
```
Type: "Restaurants in Amsterdam"
Expected: ? List of 5 restaurants
```

### Test 2: Detailed Information
```
Type: "More details about [restaurant name]"
Expected: ? Hours, cuisine, price, dishes, ambiance
```

### Test 3: Geolocation
```
Type: "Restaurants near me"
Expected: ? Nearby restaurants found
```

### Test 4: Dutch Language
```
Type: "Restaurants in Amsterdam"
Then click language switcher (EN/NL)
Expected: ? All text in Dutch
```

---

## ?? Security Checklist

- ? No API keys in `.csproj`
- ? No API keys in `appsettings.json`
- ? No API keys in `Program.cs`
- ? No API keys in `AiChatController.cs`
- ? All keys in User Secrets (secure location)
- ? User Secrets loaded in Development mode
- ? Code reads from `IConfiguration` object

---

## ?? How It Works

```
1. appsettings.json ? Empty strings for API keys
2. Program.cs ? Loads User Secrets in Development
3. IConfiguration ? Merges appsettings + user secrets
4. AiChatController ? Reads from IConfiguration
5. Groq API key ? From User Secrets
6. Google OAuth ? From User Secrets
```

---

## ?? Code Structure

### appsettings.json (Safe to commit)
```json
{
  "Groq": {
    "ApiKey": ""  // Empty - loaded from user secrets
  }
}
```

### Program.cs (Safe to commit)
```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();  // Loads from secure location
}
```

### AiChatController.cs (Safe to commit)
```csharp
var apiKey = _config["Groq:ApiKey"];  // Reads from configuration
```

---

## ? Benefits

? **Secure**: Keys not in version control
? **Flexible**: Different keys per environment
? **Standard**: Follows Microsoft best practices
? **Safe**: Keys in protected system location
? **Easy**: Simple dotnet user-secrets commands

---

## ?? Ready!

Everything is configured securely. Just:

1. ? Set user secrets (commands above)
2. ? Start both services
3. ? Open dashboard
4. ? Test commands

**Your app is production-ready!** ??
