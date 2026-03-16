using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAi.Model;
using RestaurantAi.Api.Models; // add DTO namespace
using Microsoft.Extensions.Logging;

namespace RestaurantAi.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly RestaurantAiDbContext _db;
    private readonly ILogger<ReservationsController> _log;

    public ReservationsController(RestaurantAiDbContext db, ILogger<ReservationsController> log)
    {
        _db = db;
        _log = log;
    }

    // GET api/reservations - list user's reservations
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        _log.LogInformation("GET /api/reservations called by user {UserId}", userId ?? "(anonymous)");

        if (userId == null) return Unauthorized();

        var items = await _db.Reservations.Where(r => r.OwnerId == userId).ToListAsync();
        return Ok(items);
    }

    // GET api/reservations/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _log.LogInformation("GET /api/reservations/{Id} called by user {UserId}", id, userId ?? "(anonymous)");

        var r = await _db.Reservations.FindAsync(id);
        if (r == null) return NotFound();
        if (r.OwnerId != userId) return Forbid();
        return Ok(r);
    }

    // POST api/reservations
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reservation model)
    {
        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _log.LogInformation("POST /api/reservations called by user {UserId} with model {@Model}", userId ?? "(anonymous)", model);

        if (userId == null) return Unauthorized();

        model.OwnerId = userId;
        _db.Reservations.Add(model);
        await _db.SaveChangesAsync();
        _log.LogInformation("Reservation created with id {Id} for user {UserId}", model.Id, userId);
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    // PUT api/reservations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationRequest update)
    {
        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _log.LogInformation("PUT /api/reservations/{Id} called by user {UserId} with update {@Update}", id, userId ?? "(anonymous)", update);

        var r = await _db.Reservations.FindAsync(id);
        if (r == null)
        {
            _log.LogWarning("Reservation {Id} not found", id);
            return NotFound();
        }
        if (r.OwnerId != userId)
        {
            _log.LogWarning("User {UserId} attempted to update reservation {Id} owned by {Owner}", userId, id, r.OwnerId);
            return Forbid();
        }

        r.Date = update.Date;
        r.Time = update.Time;
        r.PartySize = update.PartySize;
        r.SpecialRequests = update.SpecialRequests;

        await _db.SaveChangesAsync();
        _log.LogInformation("Reservation {Id} updated by user {UserId}", id, userId);
        return NoContent();
    }

    // DELETE api/reservations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _log.LogInformation("DELETE /api/reservations/{Id} called by user {UserId}", id, userId ?? "(anonymous)");

        var r = await _db.Reservations.FindAsync(id);
        if (r == null) return NotFound();
        if (r.OwnerId != userId) return Forbid();

        _db.Reservations.Remove(r);
        await _db.SaveChangesAsync();
        _log.LogInformation("Reservation {Id} deleted by user {UserId}", id, userId);
        return NoContent();
    }
}
