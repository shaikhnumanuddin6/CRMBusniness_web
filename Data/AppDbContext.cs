using BusinessCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessCRM.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // CRITICAL: base.OnModelCreating(builder) must be called first
            // to configure ASP.NET Core Identity keys and relationships properly.
            base.OnModelCreating(builder);
        }
    }
}
