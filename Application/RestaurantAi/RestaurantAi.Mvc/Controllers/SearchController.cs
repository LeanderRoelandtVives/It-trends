using Microsoft.AspNetCore.Mvc;

namespace RestaurantAi.Mvc.Controllers;

public class SearchController : BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Results(string query)
    {
        // TODO: Implement search logic
        ViewBag.Query = query;
        return View();
    }
}
