namespace RestaurantAi.Repository
{
    internal class RestaurantAiDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    {
        public DbSet<User> Users { get; set; } = null!;
    }
}
