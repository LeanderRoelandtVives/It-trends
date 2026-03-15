using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Dto.Request;
using RestaurantAi.Model;

namespace RestaurantAi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController(
         UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return NotFound("User not found.");

            if (!await userManager.IsInRoleAsync(user, "Admin"))
                await userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = $"{user.Email} is now an Admin." });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = userManager.Users.ToList();
            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    IsAdmin = roles.Contains("Admin")
                });
            }

            return Ok(result);
        }
    }
}
