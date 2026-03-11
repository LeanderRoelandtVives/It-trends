# OpenAI API Key Setup voor DineAI ??

## Stap 1: Verkrijg je OpenAI API Key

1. Ga naar [OpenAI Platform](https://platform.openai.com/api-keys)
2. Log in of maak een gratis account
3. Klik op **"Create new secret key"**
4. Geef een naam: `DineAI-Development`
5. **Kopieer de key onmiddellijk** (deze wordt maar één keer getoond!)

### ?? Gratis Tier Info
- Nieuwe accounts krijgen **$5 gratis credit** (geldig 3 maanden)
- **gpt-4o-mini** kost: ~$0.15 per 1M input tokens, ~$0.60 per 1M output tokens
- Voor development/testing is dit meer dan genoeg!

---

## Stap 2: Configureer API Key (VEILIG met User Secrets)

### Optie A: Via Visual Studio (Aanbevolen) ??

1. **Open Solution Explorer**
2. **Right-click** op het `RestaurantAi.Mvc` project
3. Selecteer **"Manage User Secrets"**
4. Plak de volgende JSON in het geopende bestand:

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-JOUW_ECHTE_API_KEY_HIER"
  }
}
```

5. Vervang `sk-proj-JOUW_ECHTE_API_KEY_HIER` met je echte key
6. **Save** (Ctrl+S)

### Optie B: Via Command Line (PowerShell) ??

Open PowerShell in de `RestaurantAi.Mvc` directory en voer uit:

```powershell
# Navigeer naar het project
cd C:\Users\seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Mvc

# Initialiseer User Secrets (als nog niet gedaan)
dotnet user-secrets init

# Stel je API key in
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-JOUW_ECHTE_API_KEY_HIER"

# Verifieer dat het werkt
dotnet user-secrets list
```

Je zou moeten zien:
```
OpenAI:ApiKey = sk-proj-...
```

---

## Stap 3: Verifieer de Configuratie

### Check of AIService de key oppikt:

Open `RestaurantAi.Mvc\Services\AIService.cs` en controleer of deze code aanwezig is:

```csharp
public class AIService
{
    private readonly string _apiKey;
    
    public AIService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(_apiKey))
            throw new Exception("OpenAI API Key not configured!");
    }
}
```

### Test de configuratie:

Start de applicatie en navigeer naar de **Concierge** pagina. Als alles goed is geconfigureerd, zou de AI chat moeten werken!

---

## Stap 4: Alternatieve Gratis AI Services (Als je geen OpenAI wilt gebruiken)

### ?? Gratis Alternatieven:

1. **Azure OpenAI (Studenten)**
   - Gratis credits via Azure for Students
   - Zelfde API als OpenAI
   - [Azure for Students](https://azure.microsoft.com/en-us/free/students/)

2. **Hugging Face Inference API**
   - Gratis tier beschikbaar
   - Ondersteunt vele open-source modellen
   - [Hugging Face](https://huggingface.co/inference-api)

3. **Google Gemini API**
   - Gratis tier met 60 requests/minuut
   - [Google AI Studio](https://makersuite.google.com/)

4. **Anthropic Claude (Limited Free)**
   - Beperkte gratis toegang
   - [Anthropic Console](https://console.anthropic.com/)

---

## Veelvoorkomende Problemen ??

### Probleem: "OpenAI API Key not configured!"

**Oplossing:**
```powershell
dotnet user-secrets list
```
Als leeg, voer dan uit:
```powershell
dotnet user-secrets set "OpenAI:ApiKey" "jouw-key-hier"
```

### Probleem: "401 Unauthorized"

**Oorzaken:**
- ? Ongeldige API key
- ? API key is verlopen
- ? Geen billing ingesteld op OpenAI account

**Oplossing:**
1. Verifieer je API key op [OpenAI Platform](https://platform.openai.com/api-keys)
2. Check of je billing hebt ingesteld (vereist voor gebruik na gratis credits)

### Probleem: "429 Rate Limit Exceeded"

**Oplossing:**
- Je hebt het gratis quota overschreden
- Wacht even of upgrade naar paid tier
- Voor development: switch naar `gpt-3.5-turbo` (goedkoper)

---

## Huidige Configuratie van je App ??

Je applicatie gebruikt:
- **Model**: `gpt-4o-mini` (goedkoopste GPT-4 variant)
- **Endpoint**: `https://api.openai.com/v1`
- **API Key**: Opgeslagen in User Secrets (veilig!)

---

## Security Best Practices ?

1. **NOOIT** API keys in git committen
2. **ALTIJD** User Secrets gebruiken voor development
3. **ALTIJD** Environment Variables gebruiken voor production
4. **.gitignore** controleren dat `appsettings.Development.json` wordt genegeerd als het keys bevat

### Check je .gitignore:

```gitignore
# User-specific files
*.user
*.userosscache
*.suo

# User secrets
secrets.json

# Build results
[Dd]ebug/
[Rr]elease/

# Never commit API keys!
appsettings.Development.json
appsettings.Local.json
```

---

## Volgende Stappen ??

1. ? Verkrijg OpenAI API key
2. ? Configureer via User Secrets
3. ? Start de applicatie
4. ? Test de AI Concierge chat
5. ? Geniet van je AI-powered restaurant app!

---

## Hulp Nodig? ??

- **OpenAI Documentatie**: [https://platform.openai.com/docs](https://platform.openai.com/docs)
- **ASP.NET User Secrets**: [Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- **DineAI Issues**: Check de GitHub repository

---

**Let op**: De huidige AI chat werkt nu met OpenStreetMap data (geen OpenAI nodig). 
Deze setup is voor **toekomstige AI features** zoals:
- Slimme restaurant aanbevelingen
- Natuurlijke taalverwerking voor complexe queries
- Personalisatie van suggesties

Veel succes! ??
