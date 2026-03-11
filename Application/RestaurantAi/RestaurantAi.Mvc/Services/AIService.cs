using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace RestaurantAi.Mvc.Services
{
    public class AIService
    {
        private readonly ChatClient _chatClient;
        private readonly string _systemPrompt;

        public AIService(IConfiguration configuration)
        {
            var apiKey = configuration["OpenAI:ApiKey"];
            var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

            // Check if API key is configured
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here" || apiKey == "sk-proj-your-openai-api-key-here")
            {
                // Fallback mode - will use simple responses
                _chatClient = null;
            }
            else
            {
                var client = new OpenAIClient(apiKey);
                _chatClient = client.GetChatClient(model);
            }

            _systemPrompt = @"You are a professional dining concierge AI assistant for DineAI, a restaurant discovery and booking platform.

Your role is to help users find the perfect restaurant based on their preferences.

Guidelines:
1. Be friendly, professional, and enthusiastic about food and dining
2. Ask clarifying questions about: location, cuisine type, price range, occasion, dietary restrictions
3. When users provide a location and preferences, search for restaurants that match
4. Provide restaurant recommendations with details like cuisine type, atmosphere, and highlights
5. Help users narrow down choices and make reservations
6. If users ask general questions about food or dining, answer helpfully
7. Support both English and Dutch languages seamlessly

Current capabilities:
- Restaurant search by location and cuisine
- Filtering by various criteria
- Booking assistance
- General dining advice

Be conversational and natural. Don't be overly formal. Use emojis occasionally to be friendly (but not excessively).

When a user provides a location, tell them you'll search for restaurants and ask what type of cuisine they prefer.
When you have restaurant data available, present it in a friendly and organized way.";
        }

        public async Task<string> GetResponseAsync(string userMessage, List<ChatMessage> conversationHistory)
        {
            // Fallback mode if no API key configured
            if (_chatClient == null)
            {
                return GetFallbackResponse(userMessage);
            }

            try
            {
                // Build the full conversation including system prompt
                var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(_systemPrompt)
                };

                messages.AddRange(conversationHistory);
                messages.Add(ChatMessage.CreateUserMessage(userMessage));

                // Get completion from OpenAI
                var completion = await _chatClient.CompleteChatAsync(messages);

                return completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                return $"I apologize, but I'm having trouble processing that right now. Error: {ex.Message}";
            }
        }

        public async Task<string> GetRestaurantSearchResponseAsync(
            string userMessage, 
            List<ChatMessage> conversationHistory,
            string? location = null,
            List<object>? restaurants = null)
        {
            // Fallback mode if no API key configured
            if (_chatClient == null)
            {
                if (restaurants != null && restaurants.Any())
                {
                    return $"?? Found {restaurants.Count} restaurants near {location}!\n\nI can help you explore these options. What would you like to know?";
                }
                return GetFallbackResponse(userMessage);
            }

            var contextMessage = "";

            if (!string.IsNullOrEmpty(location))
            {
                contextMessage += $"\nUser's current location context: {location}";
            }

            if (restaurants != null && restaurants.Any())
            {
                contextMessage += $"\n\nAvailable restaurants (use this data to help the user):\n{System.Text.Json.JsonSerializer.Serialize(restaurants)}";
            }

            var enhancedSystemPrompt = _systemPrompt + contextMessage;

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(enhancedSystemPrompt)
            };

            messages.AddRange(conversationHistory);
            messages.Add(ChatMessage.CreateUserMessage(userMessage));

            try
            {
                var completion = await _chatClient.CompleteChatAsync(messages);
                return completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                return $"I apologize, but I'm having trouble right now. Error: {ex.Message}";
            }
        }

        private string GetFallbackResponse(string userMessage)
        {
            var lower = userMessage.ToLowerInvariant();

            // Simple pattern matching for common questions
            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hoi") || lower.Contains("hallo"))
            {
                return "?? Hi! I'm your dining concierge. I can help you find the perfect restaurant. Where would you like to dine?";
            }

            if (lower.Contains("help") || lower.Contains("hulp"))
            {
                return "I can help you:\n• Find restaurants by location\n• Search by cuisine type\n• Get recommendations\n\nJust tell me where you'd like to eat and what you're in the mood for!";
            }

            if (lower.Contains("thank") || lower.Contains("bedankt") || lower.Contains("dank"))
            {
                return "You're very welcome! ?? Enjoy your meal!";
            }

            // Default response
            return "I'd love to help you find a great restaurant! Could you tell me:\n• Which city or area?\n• What type of cuisine?\n\n(Note: Configure OpenAI API key in appsettings.json for full AI capabilities)";
        }
    }
}
