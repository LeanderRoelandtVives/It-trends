# ?? Quick Start: OpenAI API Key Setup

## Voor de haastige ontwikkelaar! ?

### 1?? Verkrijg je API Key (2 minuten)

```
1. Ga naar: https://platform.openai.com/api-keys
2. Log in / Maak account (Google/GitHub werkt)
3. Klik: "Create new secret key"
4. Naam: "DineAI"
5. Kopieer de key (begint met sk-proj-...)
```

### 2?? Installeer de Key (30 seconden)

#### Optie A: Automatisch (Aanbevolen) ??

Open PowerShell in de project directory:

```powershell
cd C:\Users\seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Mvc
.\setup-openai.ps1
```

Volg de prompts en plak je API key.

#### Optie B: Manueel (Visual Studio) ???

```
1. Right-click op "RestaurantAi.Mvc" project
2. Klik "Manage User Secrets"
3. Plak dit (vervang de key):

{
  "OpenAI": {
    "ApiKey": "sk-proj-JOUW_KEY_HIER"
  }
}

4. Save (Ctrl+S)
```

#### Optie C: Command Line Ninja ??

```powershell
cd RestaurantAi.Mvc
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-JOUW_KEY_HIER"
```

### 3?? Test het (10 seconden)

```powershell
.\test-openai-config.ps1
```

Als je ziet: "? All tests passed!" ? Je bent klaar! ??

### 4?? Run de App

```powershell
dotnet run
```

Of druk **F5** in Visual Studio.

---

## ?? Gratis Tier Info

- **Nieuwe accounts**: $5 gratis credit (3 maanden geldig)
- **gpt-4o-mini**: ~$0.15/$0.60 per 1M tokens (in/out)
- **Voor dit project**: Maanden lang gratis bruikbaar!

---

## ?? Veelvoorkomende Fouten

### "OpenAI API Key not configured"
```powershell
dotnet user-secrets list
# Als leeg:
dotnet user-secrets set "OpenAI:ApiKey" "jouw-key"
```

### "401 Unauthorized"
- Check je API key op platform.openai.com
- Zorg dat billing is ingesteld (credit card vereist, maar gratis credits eerst)

### "429 Rate Limit"
- Gratis quota op ? wacht of upgrade
- Voor dev: switch naar `gpt-3.5-turbo` (goedkoper)

---

## ?? Security Checklist

- ? API key in User Secrets (NOOIT in code!)
- ? `appsettings.*.json` leeg van keys
- ? `.gitignore` beschermt secrets
- ? Geen keys in Git commits

---

## Klaar! ??

Je DineAI chat werkt nu met OpenStreetMap (geen OpenAI nodig).
Deze setup is voor **toekomstige AI features**!

**Vragen?** Check `OPENAI_SETUP.md` voor details.
