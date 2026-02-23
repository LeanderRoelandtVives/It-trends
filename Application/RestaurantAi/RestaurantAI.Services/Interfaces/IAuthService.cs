using RestaurantAi.Dto.Request;

namespace RestaurantAI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequest registerRequest);
        Task<string> LoginAsync(LoginRequest loginRequest);
    }
}