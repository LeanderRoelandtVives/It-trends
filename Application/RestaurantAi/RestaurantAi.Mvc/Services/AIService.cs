namespace RestaurantAi.Mvc.Services
{
    public class AIService
    {
        private readonly IConfiguration _configuration;

        public AIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetResponseAsync(string userMessage, List<dynamic> conversationHistory)
        {
            return await CallApiAsync(userMessage, conversationHistory);
        }

        public async Task<string> GetRestaurantSearchResponseAsync(
            string userMessage, 
            List<dynamic> conversationHistory,
            string? location = null,
            List<object>? restaurants = null)
        {
            return await CallApiAsync(userMessage, conversationHistory);
        }

        private async Task<string> CallApiAsync(string userMessage, List<dynamic> conversationHistory)
        {
            try
            {
                using var client = new HttpClient();
                var payload = new
                {
                    Message = userMessage,
                    SessionId = "",
                    Language = "en"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7123/api/AiChat/message", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }

                return "Sorry, I'm having trouble connecting to the AI service.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
