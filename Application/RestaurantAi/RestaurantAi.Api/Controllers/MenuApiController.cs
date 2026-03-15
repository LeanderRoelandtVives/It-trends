using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAi.Dto.Request;
using RestaurantAi.Model;
using System;

namespace RestaurantAi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuApiController(RestaurantAiDbContext db) : ControllerBase
    {
        // GET /api/menu — any logged in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var items = await db.MenuItems.OrderBy(m => m.Category).ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST /api/menu — admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] MenuItemRequest request)
        {
            var item = new MenuItem
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                Category = request.Category
            };
            db.MenuItems.Add(item);
            await db.SaveChangesAsync();
            return Ok(item);
        }

        // PUT /api/menu/{id} — admin only
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] MenuItemRequest request)
        {
            var item = await db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = request.Name;
            item.Description = request.Description;
            item.Price = request.Price;
            item.ImageUrl = request.ImageUrl;
            item.Category = request.Category;

            await db.SaveChangesAsync();
            return Ok(item);
        }

        // DELETE /api/menu/{id} — admin only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            db.MenuItems.Remove(item);
            await db.SaveChangesAsync();
            return Ok(new { message = "Deleted." });
        }
    }
}