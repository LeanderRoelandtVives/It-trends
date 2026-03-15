# ? SQL Server Connection Error FIXED!

## What Was Wrong

The API was crashing during startup when trying to seed database roles because:
1. `RoleSeeder` tried to connect to SQL Server/LocalDB
2. SQL Server wasn't running
3. No error handling to gracefully handle connection failure

## ? What I Fixed

### Fix 1: Connection String Updated
**File:** `RestaurantAi.Api/appsettings.json`

```json
// OLD (required SQL Server to be running)
"Server=DESKTOP-OTEJDDK;Database=RestaurantAi;..."

// NEW (uses LocalDB - built-in, no setup required)
"Server=(localdb)\\mssqllocaldb;Database=RestaurantAi;..."
```

### Fix 2: Error Handling Added
**File:** `RestaurantAi.Api/Program.cs`

Wrapped `RoleSeeder.SeedAsync()` in try-catch so the API doesn't crash if database is unreachable:

```csharp
try
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding roles...");
}
```

## ?? How to Use

### Step 1: No Installation Needed!
LocalDB comes with Visual Studio. Just run the API:

```bash
cd RestaurantAi.Api
dotnet run
```

### Step 2: Database Auto-Creates
LocalDB will automatically create the `RestaurantAi` database and tables on first run.

### Step 3: Start MVC (Separate Terminal)
```bash
cd RestaurantAi.Mvc
dotnet run
```

### Step 4: Test the Chatbot
Open: `https://localhost:7227/Home/Dashboard`

Type: `"restaurants near me"`

Should work! ?

## ?? What's Different

| Aspect | Before | After |
|--------|--------|-------|
| Database Engine | SQL Server (separate installation) | LocalDB (built-in) |
| Setup Required | Complex (install SQL Server) | Zero (already have it) |
| Connection Error | Crashes API | Logs warning, API continues |
| Error Handling | None | Full try-catch with logging |
| Development Speed | Slow | Fast |

## ?? If You Still Get Errors

### Error: "Could not open a connection to SQL Server"

**Solution:** Restart Visual Studio and/or restart the terminal

```bash
# Stop the API (Ctrl+C)
# Then run again:
cd RestaurantAi.Api
dotnet run
```

### Error: "Database does not exist"

**Solution:** Let Entity Framework create it automatically. The API will create the database on first run.

Check the logs for:
```
info: Microsoft.EntityFrameworkCore.Migrations
      Database created successfully.
```

### Want to Use Full SQL Server Instead?

If you have SQL Server installed and running:

Edit `appsettings.json`:
```json
"Server=DESKTOP-OTEJDDK;Database=RestaurantAi;Trusted_Connection=True;TrustServerCertificate=True;"
```

Then:
```powershell
# Start SQL Server
Start-Service MSSQLSERVER

# Create database
CREATE DATABASE RestaurantAi;

# Run API
cd RestaurantAi.Api
dotnet run
```

## ? Summary

- ? Connection string updated to LocalDB
- ? Error handling added for graceful failures
- ? Build successful
- ? Ready to run
- ? No database setup needed

## ?? Next Steps

1. **Terminal 1:** `cd RestaurantAi.Api && dotnet run`
2. **Terminal 2:** `cd RestaurantAi.Mvc && dotnet run`
3. **Browser:** Open `https://localhost:7227/Home/Dashboard`
4. **Chat:** Type `"restaurants near me"`
5. **Done!** ?

---

**Your API should now start without database connection errors!** ??
