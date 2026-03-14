using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantAi.Model;

public class RestaurantAiDbContext : IdentityDbContext<ApplicationUser>
{
    public RestaurantAiDbContext(DbContextOptions<RestaurantAiDbContext> options)
        : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems { get; set; }
}