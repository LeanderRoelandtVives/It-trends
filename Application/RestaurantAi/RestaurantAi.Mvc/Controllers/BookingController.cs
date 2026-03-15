using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Models;

namespace RestaurantAi.Mvc.Controllers;

public class BookingController : BaseController
{
    // Simple in-memory store for demo purposes
    private static readonly List<ReservationViewModel> _store = new List<ReservationViewModel>();
    private static readonly object _lock = new object();

    [HttpGet]
    public IActionResult Index()
    {
        // Show bookings list by default when visiting /Booking
        return RedirectToAction("List");
    }

    [HttpGet]
    public IActionResult New()
    {
        // Render the reservation form (reuse Views/Booking/Index.cshtml)
        var model = new ReservationViewModel
        {
            Date = DateTime.Today,
            Time = "7:00 PM",
            PartySize = 2
        };
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ReservationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        int id;
        lock (_lock)
        {
            _store.Add(model);
            id = _store.Count - 1;
        }

        // Store reservation in TempData for immediate display fallback
        TempData["Reservation"] = JsonSerializer.Serialize(model);

        return RedirectToAction("Details", new { id });
    }

    [HttpGet]
    public IActionResult Details(int? id)
    {
        ReservationViewModel? model = null;

        if (id.HasValue)
        {
            lock (_lock)
            {
                if (id.Value >= 0 && id.Value < _store.Count)
                {
                    model = _store[id.Value];
                }
            }
        }

        // fallback to TempData payload (recently created reservation)
        if (model == null && TempData.TryGetValue("Reservation", out var obj) && obj is string json)
        {
            try
            {
                model = JsonSerializer.Deserialize<ReservationViewModel>(json);
            }
            catch
            {
                // ignore
            }
        }

        if (model != null)
        {
            return View(model);
        }

        // If no reservation data available, redirect back to index
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult List()
    {
        List<ReservationViewModel> snapshot;
        lock (_lock)
        {
            snapshot = _store.ToList();
        }
        return View(snapshot);
    }
}
