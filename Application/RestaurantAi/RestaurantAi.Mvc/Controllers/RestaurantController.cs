using Microsoft.AspNetCore.Mvc;

namespace RestaurantAi.Mvc.Controllers;

public class RestaurantController : BaseController
{
    [HttpGet]
    public IActionResult Details(int id)
    {
        // TODO: Implement restaurant details logic
        ViewBag.RestaurantId = id;
        return View();
    }

    [HttpGet]
    public IActionResult Menu(int id)
    {
        // TODO: Implement menu view logic
        return View();
    }
}
