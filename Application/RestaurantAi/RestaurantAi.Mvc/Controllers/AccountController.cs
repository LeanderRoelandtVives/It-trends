using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Models;

namespace RestaurantAi.Mvc.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // TODO: Implement authentication logic
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // TODO: Implement registration logic
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }
}
