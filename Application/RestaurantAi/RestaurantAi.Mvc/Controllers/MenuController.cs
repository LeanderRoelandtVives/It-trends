using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Models;

namespace RestaurantAi.Mvc.Controllers
{
    public class MenuController : BaseController
    {
        private readonly HttpClient _client;

        public MenuController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("RestaurantApi");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            List<MenuItemViewModel> items = new();

            var response = await _client.GetAsync("api/menuApi");
            if (response.IsSuccessStatusCode)
            {
                items = await response.Content.ReadFromJsonAsync<List<MenuItemViewModel>>() ?? new List<MenuItemViewModel>();
            }

            // Group by category for the view
            var grouped = items
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(grouped);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");
            return View(new MenuItemViewModel());
        }

        // POST — submit create form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItemViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _client.PostAsJsonAsync("api/menuApi", model);
            return RedirectToAction("Index");
        }

        // GET — show edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var response = await _client.GetAsync($"api/menuApi/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var item = await response.Content.ReadFromJsonAsync<MenuItemViewModel>();
            return View(item);
        }

        // POST — submit edit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItemViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _client.PutAsJsonAsync($"api/menuApi/{id}", model);
            return RedirectToAction("Index");
        }

        // POST — delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"api/menuApi/{id}");
            return RedirectToAction("Index");
        }
    }
}
