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
}

// Helper class to deserialize API token response
public class TokenResponse
{
    public string Token { get; set; }
}