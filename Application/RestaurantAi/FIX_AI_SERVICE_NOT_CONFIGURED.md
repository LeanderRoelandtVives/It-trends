# ?? Fix: "AI Service is Not Configured"

## The Problem

When you ask "Tell me more details", the system says:
```
AI service is not configured
```

This means the **Groq API key is not being read** by the application.

## The Solution

### Step 1: Verify User Secrets Are Initialized

Run this command:
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet user-secrets init
```

**Output should be:**
```
The user secrets 'secrets.json' file doesn't exist yet. 
Would you like to create it? (yes/no) yes
Successfully created the 'secrets.json' file.
```

### Step 2: Check Your Groq API Key

Verify you have your Groq API key. If not:
1. Go to https://console.groq.com/keys
2. Sign in or create account
3. Copy your API key (starts with `gsk_`)

### Step 3: Set the User Secret

Run this command (replace YOUR_KEY with your actual key):
```bash
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY_HERE"
```


**Example:**
```bash
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY_HERE"
```

### Step 4: Verify the Secret Was Set

```bash
dotnet user-secrets list
```


**You should see:**
```
Groq:ApiKey = YOUR_KEY_HERE
```

### Step 5: Restart Your API

```bash
# Stop current API (Ctrl+C)
# Then restart it:
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api
dotnet run
```

### Step 6: Test It

1. Open Dashboard: `https://localhost:7227/Home/Dashboard`
2. Search for restaurants: `"Restaurants in Brussels"`
3. Ask for details: `"Tell me more details"`
4. **Should now work!** ?

## Troubleshooting

### "secrets.json file doesn't exist"

**Solution:** Run the init command from Step 1:
```bash
dotnet user-secrets init
```

### Still seeing "AI service is not configured"

**Check the API logs:**
1. Look in the API terminal for error messages
2. Should see something like:
   ```
   Checking Groq API key - Key present: True
   ```

   If it says `False`, the key isn't set.

### Can't find secrets.json

Location: `C:\Users\Seppe\AppData\Roaming\Microsoft\UserSecrets\[project-id]\secrets.json`

**To find your project ID:**
```bash
cd RestaurantAi.Api
dotnet user-secrets id
```

This shows your project's unique ID.

## Quick Check Commands

**Check if secret is set:**
```bash
dotnet user-secrets list
```

**Check configuration:**
```bash
dotnet user-secrets get "Groq:ApiKey"
```

**Remove a secret:**
```bash
dotnet user-secrets remove "Groq:ApiKey"
```

**Clear all secrets:**
```bash
dotnet user-secrets clear
```

## The Complete Steps (Fresh Start)

If nothing works, do this complete reset:

```bash
# Navigate to API project
cd C:\Users\Seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Api

# Clear all secrets
dotnet user-secrets clear

# Initialize user secrets
dotnet user-secrets init

# Set your API key (replace with your actual key)
dotnet user-secrets set "Groq:ApiKey" "gsk_YOUR_KEY_HERE"

# Verify it was set
dotnet user-secrets list

# Restart the API
dotnet run

# In another terminal, test it
# Open: https://localhost:7227/Home/Dashboard
# Try: "restaurants in brussels"
# Then: "Tell me more details"
```

## Expected Output After Fix

**In API terminal:**
```
Checking Groq API key - Key present: True
Configuration source - Groq:ApiKey value: 42 chars
Sending request to Groq API for restaurant details
Successfully retrieved restaurant details from Groq
```

**In browser:**
```
Here are some restaurants I found:
1. Restaurant Name � Brussels
   https://maps.google.com/...
...

Would you like more details or to make a reservation?

User: "Tell me more details"

AI: "Brussels offers a diverse culinary scene with both traditional 
Belgian cuisine and international dining options. When visiting restaurants 
in Brussels, expect high-quality ingredients, attentive service, and a rich 
dining culture. Popular Belgian dishes include waffles, fries, seafood, and 
locally-sourced products. Most restaurants in the area feature cozy atmospheres 
and professional service. Budget varies from casual cafes to fine dining 
establishments..."
```

## Pro Tips

? **Keep your API key secure**
- Never commit it to Git
- Never share it in documentation
- User secrets are stored locally only

? **Test each step**
- After setting the secret, run `dotnet user-secrets list`
- Verify it shows your key
- Only then restart the API

? **Different keys for different environments**
- Development: User secrets (what we're doing)
- Production: Environment variables
- CI/CD: GitHub Secrets

## Still Not Working?

**Check these:**

1. **Project ID mismatch**
   ```bash
   dotnet user-secrets id
   ```
   Make sure you're setting secrets for the right project

2. **Environment not set to Development**
   Add to your shell:
   ```bash
   $env:ASPNETCORE_ENVIRONMENT="Development"
   ```

3. **API key format**
   Should start with `gsk_` and be ~50+ characters

4. **Restart is required**
   Always restart the API after setting a secret

## File Locations

**User Secrets:**
```
C:\Users\Seppe\AppData\Roaming\Microsoft\UserSecrets\[project-id]\secrets.json
```

**API appsettings:**
```
RestaurantAi.Api\appsettings.json
(The Groq:ApiKey there should be empty)
```

**Program.cs:**
```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
```

---

## Quick Summary

```bash
# 1. Initialize
dotnet user-secrets init

# 2. Set key
dotnet user-secrets set "Groq:ApiKey" "YOUR_KEY"

# 3. Verify
dotnet user-secrets list

# 4. Restart API
dotnet run

# 5. Test in browser
# Open dashboard and try "Tell me more details"
```

That's it! Your "AI service is not configured" error should be fixed! ??
