using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RestaurantAi.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _client;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("RestaurantApi");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await _client.PostAsJsonAsync("api/auth/login", new
        {
            Email = model.Email,
            Password = model.Password
        });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Ongeldige loginpoging.");
            return View(model);
        }

        // Deserialize token
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        HttpContext.Session.SetString("JWToken", result.Token);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await _client.PostAsJsonAsync("api/auth/register", new
        {
            Email = model.Email,
            Password = model.Password,
            FullName = model.FirstName + " " + model.LastName
        });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Registratie mislukt.");
            return View(model);
        }

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        HttpContext.Session.SetString("JWToken", result.Token);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JWToken");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // Redirects the browser to Google's login page
    [HttpGet]
    public IActionResult LoginWithGoogle()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Account");
        var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };
        return Challenge(properties, "Google");
    }

    // Google redirects back here after the user logs in
    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        // Read the Google identity from the cookie set by the middleware
        var result = await HttpContext.AuthenticateAsync(
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Google login failed.");
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var fullName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login");

        // Call your own API to get a JWT (same as normal login)
        var response = await _client.PostAsJsonAsync("api/auth/google-login", new
        {
            Email = email,
            FullName = fullName
        });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Could not complete Google login.");
            return RedirectToAction("Login");
        }

        var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();
        HttpContext.Session.SetString("JWToken", tokenResult!.Token);

        return RedirectToAction("Index", "Home");
    }
}

// Helper class to deserialize API token response
public class TokenResponse
{
    public string Token { get; set; }
}