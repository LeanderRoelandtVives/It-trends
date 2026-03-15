# ?? Quick Fix - SQL Server Connection

## The Problem

```
Error: Error Locating Server/Instance Specified
Reason: Connection string was looking for wrong server
Old: LAPTOP-POTATO\VIVES
Current: DESKTOP-OTEJDDK
```

## ? What I Fixed

Updated `RestaurantAi.Api/appsettings.json`:
```json
"Server=DESKTOP-OTEJDDK;Database=RestaurantAi;Trusted_Connection=True;TrustServerCertificate=True;"
```

## ?? What You Need To Do

### Option 1: Use SQL Server (If Installed)

```powershell
# 1. Check if SQL Server is running
Get-Service MSSQLSERVER

# 2. If Status is not "Running", start it:
Start-Service MSSQLSERVER

# 3. Create database (in SQL Server Management Studio):
CREATE DATABASE RestaurantAi;

# 4. Run the API
cd RestaurantAi.Api
dotnet run
```

### Option 2: Use LocalDB (Simpler - No Installation)

**No action needed!** Just update one line:

Edit `RestaurantAi.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "RestaurantAiDbContext": "Server=(localdb)\\mssqllocaldb;Database=RestaurantAi;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Then run:
```bash
cd RestaurantAi.Api
dotnet run
```

LocalDB will automatically create the database! ?

## ?? Checklist

- ? Connection string updated to `DESKTOP-OTEJDDK`
- ? SQL Server running (or using LocalDB)
- ? Database `RestaurantAi` exists
- ? API started: `cd RestaurantAi.Api && dotnet run`
- ? MVC started (separate terminal): `cd RestaurantAi.Mvc && dotnet run`
- ? Chatbot works at `https://localhost:7227`

## ?? Recommendation

Use **LocalDB** - it's simpler:
1. Change connection string to `(localdb)\mssqllocaldb`
2. No need to install/manage SQL Server
3. Comes with Visual Studio
4. Perfect for development

---

**Try Option 2 first (LocalDB) - it's easier!** ?
