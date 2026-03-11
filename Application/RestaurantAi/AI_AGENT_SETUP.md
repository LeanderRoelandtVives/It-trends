# DineAI - Real AI Agent Setup

## ?? AI-Powered Restaurant Concierge

Your chatbot now uses **OpenAI GPT-4** to respond intelligently to ANY question!

## Setup Instructions

### 1. Get an OpenAI API Key

1. Go to [https://platform.openai.com/api-keys](https://platform.openai.com/api-keys)
2. Sign up or log in
3. Click "Create new secret key"
4. Copy the key (starts with `sk-proj-...`)

### 2. Add Your API Key

**Option A: Using appsettings.Development.json (Recommended for development)**

Edit `RestaurantAi.Mvc/appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR-ACTUAL-API-KEY-HERE",
    "Model": "gpt-4o-mini"
  }
}
```

**Option B: Using Environment Variables (Recommended for production)**

```bash
# Windows (PowerShell)
$env:OpenAI__ApiKey="sk-proj-YOUR-ACTUAL-API-KEY-HERE"

# Linux/Mac
export OpenAI__ApiKey="sk-proj-YOUR-ACTUAL-API-KEY-HERE"
```

**Option C: Using User Secrets (Most secure for development)**

```bash
cd RestaurantAi.Mvc
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-YOUR-ACTUAL-API-KEY-HERE"
```

### 3. Choose Your Model

Edit the `Model` setting in appsettings.json:

- **`gpt-4o-mini`** - Fastest, cheapest, recommended for development ($0.15/1M tokens)
- **`gpt-4o`** - More capable, better for complex questions ($2.50/1M tokens)
- **`gpt-4-turbo`** - Previous generation, still very capable

### 4. Test Your AI Agent

1. **Restart the application** (Ctrl+Shift+F5)
2. Go to the **Concierge** page
3. Try ANY question:
   - "What's the best Italian restaurant in Brussels?"
   - "I want seafood in Amsterdam, not too expensive"
   - "Tell me about Belgian cuisine"
   - "What's a good romantic restaurant for an anniversary?"
   - "I'm vegetarian, what do you recommend?"

## ?? What's New?

### ? Real AI Responses
- The chatbot now uses GPT to understand **ANY** question
- No more hardcoded conversation flows
- Natural, intelligent responses

### ? Smart Restaurant Search
- Still searches OpenStreetMap for real restaurants
- AI helps users refine their search
- Provides context-aware recommendations

### ? Bilingual Support
- Responds in English or Dutch based on user's language setting
- Seamlessly switches languages

### ? Context-Aware
- Remembers conversation history
- Can answer follow-up questions
- Understands user preferences

## ?? Example Conversations

**English:**
```
You: I'm looking for a restaurant in Brussels
AI: Great choice! Brussels has amazing dining options. What type of cuisine are you in the mood for? Italian, French, Belgian classics, or something else?

You: Something romantic for an anniversary
AI: Perfect! Let me search for romantic restaurants in Brussels. I'd recommend looking for French or upscale Belgian restaurants. Would you like me to show you some options?
```

**Nederlands:**
```
Jij: Ik zoek een restaurant in Antwerpen
AI: Geweldig! Antwerpen heeft fantastische restaurants. Waar heb je zin in? Italiaans, Frans, Belgisch, of iets anders?

Jij: Italiaans, niet te duur
AI: Prima! Ik ga voor je zoeken naar betaalbare Italiaanse restaurants in Antwerpen. Momentje...
```

## ?? Configuration Options

### Cost Control

The AI service is **pay-per-use**. To control costs:

1. Use `gpt-4o-mini` for development (cheapest)
2. Set a usage limit in your OpenAI account settings
3. Monitor usage at [https://platform.openai.com/usage](https://platform.openai.com/usage)

### System Prompt Customization

Edit `RestaurantAi.Mvc/Services/AIService.cs` to customize the AI's personality and capabilities:

```csharp
_systemPrompt = @"You are a professional dining concierge...";
```

## ?? Advanced Features

The AI agent can now:
- Answer general food and dining questions
- Provide cooking tips or recipe suggestions
- Recommend wine pairings
- Explain different cuisines
- Help with dietary restrictions
- Suggest restaurants for specific occasions

## ?? How It Works

1. **User sends message** ? Frontend sends to `/api/AiChat/message`
2. **Controller receives message** ? Checks for location keywords
3. **If location detected** ? Geocodes and searches OpenStreetMap
4. **AI generates response** ? Uses GPT with conversation history + restaurant data
5. **Response sent back** ? Displayed in chat with restaurant cards

## ?? Important Notes

- **API Key Security**: Never commit your API key to Git! Use `.gitignore` for `appsettings.Development.json`
- **Cost**: OpenAI charges per token. GPT-4o-mini is very affordable (~$0.10-0.50/day for moderate use)
- **Rate Limits**: Free tier has lower rate limits. Upgrade to Tier 1 for production use
- **Privacy**: Conversation history is stored in memory (not persisted to database)

## ?? Troubleshooting

### "OpenAI:ApiKey is missing"
- Make sure you added the API key to `appsettings.Development.json`
- Restart the application after adding the key

### "401 Unauthorized"
- Your API key is invalid or expired
- Check you copied the full key (starts with `sk-proj-` or `sk-`)
- Verify the key at [https://platform.openai.com/api-keys](https://platform.openai.com/api-keys)

### "429 Too Many Requests"
- You've exceeded your rate limit
- Wait a few minutes or upgrade your OpenAI account tier

### AI responses are slow
- Try switching to `gpt-4o-mini` (faster model)
- Check your internet connection
- OpenAI API may be experiencing high load

## ?? Next Steps

- Add conversation persistence to database
- Implement user preferences and favorites
- Add real booking integration
- Create admin dashboard for AI monitoring
- Implement function calling for complex restaurant queries

---

**Need help?** Check the OpenAI documentation: [https://platform.openai.com/docs](https://platform.openai.com/docs)
