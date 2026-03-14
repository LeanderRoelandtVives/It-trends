using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RestaurantAi.Mvc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiChatController : ControllerBase
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _endpoint;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();

        public AiChatController(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
            _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
            _endpoint = configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1";
        }

        public class ChatRequest 
        { 
            public string Message { get; set; } 
            public string SessionId { get; set; } 
            public string Language { get; set; } 
        }

        public class ChatResponse 
        { 
            public string Reply { get; set; } 
            public string SessionId { get; set; } 
        }

        public class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        [HttpPost("message")]
        public async Task<IActionResult> PostMessage([FromBody] ChatRequest req)
        {
            try
            {
                var sessionId = string.IsNullOrEmpty(req?.SessionId) 
                    ? Guid.NewGuid().ToString("N") 
                    : req.SessionId;

                var language = req?.Language ?? "en";
                var userMessage = req?.Message ?? "";

                // Get or create conversation history
                var conversation = _conversations.GetOrAdd(sessionId, _ => new List<ChatMessage>());

                // Set system message based on language
                if (conversation.Count == 0 || userMessage == "")
                {
                    var systemPrompt = language == "nl"
                        ? "Je bent een vriendelijke restaurant conciërge AI-assistent. Help gebruikers bij het vinden van restaurants. Wees behulpzaam, vriendelijk en geef duidelijke aanbevelingen. Antwoord altijd in het Nederlands."
                        : "You are a friendly restaurant concierge AI assistant. Help users find restaurants. Be helpful, friendly, and provide clear recommendations. Always respond in English.";

                    conversation.Clear();
                    conversation.Add(new ChatMessage 
                    { 
                        role = "system", 
                        content = systemPrompt 
                    });

                    // If empty message (initialization), return welcome
                    if (string.IsNullOrEmpty(userMessage))
                    {
                        var welcomeMsg = language == "nl"
                            ? "Hoi! ?? Ik ben je restaurant conciërge AI. Ik help je graag bij het vinden van het perfecte restaurant.\n\nWaar kan ik je mee helpen? Je kunt me vertellen:\n• In welke stad je een restaurant zoekt\n• Welk type keuken je wilt (bijv. Italiaans, Japans)\n• Je budget of speciale voorkeuren"
                            : "Hi! ?? I'm your restaurant concierge AI. I'd love to help you find the perfect restaurant.\n\nHow can I help you? You can tell me:\n• Which city you're looking for a restaurant in\n• What type of cuisine you want (e.g., Italian, Japanese)\n• Your budget or special preferences";

                        return Ok(new ChatResponse 
                        { 
                            Reply = welcomeMsg, 
                            SessionId = sessionId 
                        });
                    }
                }

                // Add user message to conversation
                conversation.Add(new ChatMessage 
                { 
                    role = "user", 
                    content = userMessage 
                });

                // Call OpenAI API
                var response = await CallOpenAI(conversation);

                // Add assistant response to conversation
                conversation.Add(new ChatMessage 
                { 
                    role = "assistant", 
                    content = response 
                });

                // Keep conversation history limited (last 20 messages)
                if (conversation.Count > 21) // system + 20 messages
                {
                    conversation.RemoveRange(1, conversation.Count - 21);
                }

                return Ok(new ChatResponse 
                { 
                    Reply = response, 
                    SessionId = sessionId 
                });
            }
            catch (Exception ex)
            {
                var errorMsg = req?.Language == "nl"
                    ? $"Sorry, er is een fout opgetreden: {ex.Message}"
                    : $"Sorry, an error occurred: {ex.Message}";

                return Ok(new ChatResponse 
                { 
                    Reply = errorMsg, 
                    SessionId = req?.SessionId ?? "" 
                });
            }
        }

        private async Task<string> CallOpenAI(List<ChatMessage> messages)
        {
            var requestBody = new
            {
                model = _model,
                messages = messages,
                temperature = 0.7,
                max_tokens = 800
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return result
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        [HttpPost("clear")]
        public IActionResult ClearSession([FromBody] ChatRequest req)
        {
            if (!string.IsNullOrEmpty(req?.SessionId))
            {
                _conversations.TryRemove(req.SessionId, out _);
            }
            return Ok(new { success = true });
        }
    }
}
