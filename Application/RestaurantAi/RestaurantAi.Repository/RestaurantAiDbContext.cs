using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantAi.Model;

namespace RestaurantAi.Repository
{
    public class RestaurantAiDbContext(DbContextOptions<RestaurantAiDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {

    }
}