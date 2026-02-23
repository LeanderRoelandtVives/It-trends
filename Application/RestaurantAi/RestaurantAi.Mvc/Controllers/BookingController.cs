using Microsoft.AspNetCore.Mvc;

namespace RestaurantAi.Mvc.Controllers;

public class BookingController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create()
    {
        // TODO: Implement booking creation logic
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        // TODO: Implement booking details logic
        return View();
    }
}
