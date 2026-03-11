# Quick Start Guide - AI Agent

## ? What's Been Implemented

Your DineAI chatbot is now powered by **OpenAI GPT-4**! It can respond intelligently to ANY question about restaurants, food, and dining.

## ?? Try It Right Now (Without API Key)

The system works in **fallback mode** without an OpenAI API key - you can test it immediately!

1. **Restart the app** (Ctrl+Shift+F5)
2. Go to **Concierge** page
3. Try these:
   - "Hello"
   - "I want Italian food in Brussels"
   - "Help me find a restaurant"

You'll get basic responses that work without AI.

## ?? Get Full AI Power (5 minutes)

### Step 1: Get Free OpenAI API Key

1. Visit: https://platform.openai.com/api-keys
2. Sign up (free $5 credit included!)
3. Click "Create new secret key"
4. Copy the key (starts with `sk-proj-`)

### Step 2: Add Your Key

Open `RestaurantAi.Mvc/appsettings.Development.json` and replace:

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-paste-your-actual-key-here",
    "Model": "gpt-4o-mini"
  }
}
```

### Step 3: Restart & Test

1. Stop the app (Shift+F5)
2. Start again (F5)
3. Ask the AI ANYTHING:
   - "What's the best Belgian beer to pair with moules-frites?"
   - "I'm vegetarian and looking for a romantic restaurant in Antwerp"
   - "Tell me about Michelin-starred restaurants"
   - "What should I order at a French bistro?"

## ?? What Can It Do Now?

### ? Natural Conversations
- Understands context and follow-up questions
- Remembers previous messages
- Works in English and Dutch

### ? Restaurant Search
- Still uses OpenStreetMap for real restaurants
- AI helps refine search criteria
- Provides intelligent recommendations

### ? General Knowledge
- Answers food and dining questions
- Explains cuisines and dishes
- Gives cooking tips
- Suggests wine pairings

### ? Smart Assistance
- Handles dietary restrictions
- Recommends for specific occasions
- Understands price preferences
- Filters by distance and location

## ?? Cost

- **gpt-4o-mini**: ~$0.15 per 1M tokens (VERY cheap!)
- Free tier includes $5 credit
- Typical conversation: 1,000-5,000 tokens = $0.0015-$0.0075
- **You can have ~1,000 conversations with $5 credit**

## ?? Configuration

**Fastest Model (Recommended):**
```json
"Model": "gpt-4o-mini"
```

**Most Capable:**
```json
"Model": "gpt-4o"
```

## ?? Test Scenarios

Try these to see the AI in action:

**English:**
- "I'm celebrating my birthday, suggest upscale restaurants in Brussels"
- "I have a gluten allergy, what Italian restaurants can accommodate me?"
- "What's the difference between Belgian waffles and Liège waffles?"

**Nederlands:**
- "Ik zoek een romantisch restaurant in Gent voor een verjaardag"
- "Wat is typisch Vlaams eten?"
- "Waar kan ik goedkoop maar lekker eten in Antwerpen?"

## ?? Important

- **Never commit API keys to Git!** 
- The `.gitignore` should exclude `appsettings.Development.json`
- Use User Secrets or Environment Variables for production

## ?? Troubleshooting

**"OpenAI:ApiKey is missing"**
- The app works in fallback mode
- Add API key to get full AI features

**"401 Unauthorized"**
- Invalid API key
- Check you copied the complete key

**"429 Rate Limit"**
- Free tier limits reached
- Wait a few minutes
- Upgrade to paid tier for higher limits

## ?? Learn More

Full documentation: `AI_AGENT_SETUP.md`

OpenAI Platform: https://platform.openai.com/

API Documentation: https://platform.openai.com/docs

---

**Ready to test?** Just restart the app and start chatting! ??
