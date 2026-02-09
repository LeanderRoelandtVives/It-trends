using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantAi.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService authService;

        public AccountController(AuthSe rvice authService)
        {
            _authService = authService;
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var user = await _authService.RegisterAsync(request);
            if (user == null)
            {
                ModelState.AddModelError("", "Username or email already exists.");
                return View(request);
            }

            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var token = await _authService.LoginAsync(request);
            if (token == null)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(request);
            }

            // Store token in cookie/session if needed
            HttpContext.Session.SetString("JWToken", token);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            return RedirectToAction("Login");
        }
    }
}
