using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RestaurantAi.Mvc.Models;
using System.Security.Claims;
using RestaurantAi.Mvc.Services;

namespace RestaurantAi.Mvc.Controllers;

public class BookingController : BaseController
{
    private readonly AuthApiClient _api;
    private readonly ILogger<BookingController> _log;

    public BookingController(AuthApiClient api, ILogger<BookingController> log)
    {
        _api = api;
        _log = log;
    }

    private string? GetCurrentUserId()
    {
        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        catch
        {
            return null;
        }
    }

    private bool IsAdmin()
    {
        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.Any(c =>
                c.Type == System.Security.Claims.ClaimTypes.Role &&
                c.Value == "Admin");
        }
        catch { return false; }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return RedirectToAction("List");
    }

    [HttpGet]
    public IActionResult New()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            var returnUrl = Url.Action("New", "Booking");
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

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
    public async Task<IActionResult> Create(ReservationViewModel model)
    {
        _log.LogInformation("BookingController.Create called with model: {@Model}", model);

        if (!ModelState.IsValid)
        {
            _log.LogWarning("ModelState invalid: {@Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View("Index", model);
        }

        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var r = new Reservation
        {
            OwnerId = userId,
            Date = model.Date,
            Time = model.Time,
            PartySize = model.PartySize,
            SpecialRequests = model.SpecialRequests
        };

        var created = await _api.CreateReservationAsync(r);
        if (created == null)
        {
            _log.LogWarning("CreateReservationAsync returned null for model {@Model}", model);
            ModelState.AddModelError(string.Empty, "Could not create reservation. Controleer of de API draait en je ingelogd bent.");
            TempData["Error"] = "Could not create reservation. Controleer of de API bereikbaar is en je ingelogd bent.";
            return View("Index", model);
        }

        TempData["Success"] = "Reservation created.";
        _log.LogInformation("Reservation created with id {Id}", created.Id);
        return RedirectToAction("Details", new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var response = await _api.GetReservationsAsync();
        if (response == null) return NotFound();

        var item = response.FirstOrDefault(r => r.Id == id);
        if (item == null) return NotFound();

        var model = new ReservationViewModel
        {
            Id = item.Id.ToString(),
            OwnerId = item.OwnerId,
            Date = item.Date,
            Time = item.Time,
            PartySize = item.PartySize,
            SpecialRequests = item.SpecialRequests
        };

        if (model.OwnerId != userId && !IsAdmin()) return Forbid();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var reservations = await _api.GetReservationsAsync();

        var filtered = IsAdmin()
            ? reservations ?? new List<Reservation>()
            : reservations?.Where(r => r.OwnerId == userId).ToList()
              ?? new List<Reservation>();

        var viewModels = filtered.Select(r => new ReservationViewModel
        {
            Id = r.Id.ToString(),
            OwnerId = r.OwnerId,
            Date = r.Date,
            Time = r.Time,
            PartySize = r.PartySize,
            SpecialRequests = r.SpecialRequests
        }).ToList();

        ViewBag.IsAdmin = IsAdmin();
        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var reservations = await _api.GetReservationsAsync();
        var r = reservations?.FirstOrDefault(x => x.Id == id);
        if (r == null) return NotFound();
        if (r.OwnerId != userId && !IsAdmin()) return Forbid();

        var model = new ReservationViewModel
        {
            Id = r.Id.ToString(),
            OwnerId = r.OwnerId,
            Date = r.Date,
            Time = r.Time,
            PartySize = r.PartySize,
            SpecialRequests = r.SpecialRequests
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReservationViewModel model)
    {
        _log.LogInformation("BookingController.Edit called with id {Id} and model {@Model}", id, model);

        if (!ModelState.IsValid)
        {
            _log.LogWarning("ModelState invalid on Edit: {@Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var update = new Reservation
        {
            Date = model.Date,
            Time = model.Time,
            PartySize = model.PartySize,
            SpecialRequests = model.SpecialRequests
        };

        var result = await _api.UpdateReservationAsync(id, update);
        if (!result.Success)
        {
            _log.LogWarning("UpdateReservationAsync returned failure for id {Id}. Status: {Status}, Body: {Body}", id, result.StatusCode, result.Body);
            ModelState.AddModelError(string.Empty, "Could not update reservation. Controleer of de API draait en je ingelogd bent.");
            TempData["Error"] = $"Update failed ({result.StatusCode}): {result.Body}";
            return View(model);
        }

        TempData["Success"] = "Reservation updated.";
        _log.LogInformation("Reservation {Id} updated successfully", id);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var ok = await _api.DeleteReservationAsync(id);
        if (!ok) return BadRequest();

        return RedirectToAction("List");
    }
}