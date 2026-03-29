using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Models;

namespace RestaurantAi.Mvc.Controllers
{
    public class AdminController : BaseController
    {
        private readonly HttpClient _client;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("RestaurantApi");
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (ViewData["IsAdmin"] is not true)
                return RedirectToAction("Index", "Menu");

            var response = await _client.GetAsync("api/admin/users");
            if (!response.IsSuccessStatusCode)
                return View(new List<UserViewModel>());

            var users = await response.Content.ReadFromJsonAsync<List<UserViewModel>>();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAdmin(string userId)
        {
            if (ViewData["IsAdmin"] is not true)
                return RedirectToAction("Index", "Menu");

            await _client.PostAsJsonAsync("api/admin/assign-role", new { UserId = userId });
            return RedirectToAction("Users");
        }
    }
}