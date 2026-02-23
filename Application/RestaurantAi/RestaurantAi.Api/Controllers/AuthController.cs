using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Dto.Request;
using RestaurantAI.Services.Services;

namespace RestaurantAi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var token = await authService.RegisterAsync(registerRequest);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var token = await authService.LoginAsync(loginRequest);
            return Ok(new { token });
        }
    }
}