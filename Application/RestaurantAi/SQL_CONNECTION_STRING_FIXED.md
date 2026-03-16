# ? SQL Server Connection Fixed!

## What Was Wrong

```
Old connection string: Server=LAPTOP-POTATO\VIVES;...
Your actual server: DESKTOP-OTEJDDK
Result: SQL Server not found error ?
```

## The Fix

Updated `appsettings.json`:
```json
"ConnectionStrings": {
  "RestaurantAiDbContext": "Server=DESKTOP-OTEJDDK;Database=RestaurantAi;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## ? Next Steps

### Step 1: Verify SQL Server is Running

**Option A: Using SQL Server Management Studio**
1. Open SQL Server Management Studio
2. Connect to `DESKTOP-OTEJDDK`
3. Should connect successfully

**Option B: Using Command Line**
```powershell
# Check if SQL Server service is running
Get-Service MSSQLSERVER, SQLEXPRESS | Select-Object Name, Status

# Should see Status: Running
```

**Option C: Start SQL Server if not running**
```powershell
# Start default SQL Server instance
Start-Service MSSQLSERVER

# Or if using Express Edition:
Start-Service SQLEXPRESS
```

### Step 2: Create the Database

```sql
-- In SQL Server Management Studio or Query window:
CREATE DATABASE RestaurantAi;
```

Or let Entity Framework create it automatically:

### Step 3: Run the API

```bash
cd RestaurantAi.Api
dotnet run
```

Should see:
```
Application started. Press Ctrl+C to shut down.
Now listening on: https://localhost:7179
```

### Step 4: Run the MVC

```bash
cd RestaurantAi.Mvc
dotnet run
```

Should see:
```
Application started. Press Ctrl+C to shut down.
Now listening on: https://localhost:7227
```

### Step 5: Test

Open: `https://localhost:7227/Home/Dashboard`

Chatbot should work! ?

## Troubleshooting

**Issue:** Still getting connection error
**Solution:**
1. Check SQL Server is running: `Get-Service MSSQLSERVER`
2. Check database exists in SQL Server
3. Verify computer name: `hostname` (should be `DESKTOP-OTEJDDK`)

**Issue:** Can't connect to SQL Server Management Studio
**Solution:**
1. SQL Server may not be installed
2. Install SQL Server Express from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
3. Or use LocalDB (simpler option - see below)

## Alternative: Use LocalDB (Simpler)

If you don't have SQL Server installed, use LocalDB:

Update connection string:
```json
"ConnectionStrings": {
  "RestaurantAiDbContext": "Server=(localdb)\\mssqllocaldb;Database=RestaurantAi;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

LocalDB comes with Visual Studio and is easier to set up.

## Status

? Connection string updated to: `DESKTOP-OTEJDDK`
?? Next: Verify SQL Server is running
?? Next: Create database (or let EF create it)
?? Next: Run API and MVC

---

**Let me know if you get another connection error!**
