using Microsoft.EntityFrameworkCore;

namespace workout_progress.Models // Adjust the namespace according to your project structure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //public DbSet<User> Users { get; set; } // Add DbSet properties for your entities
        // Add other DbSets for your entities as needed
    }
}
