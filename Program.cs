using BusinessCRM.Data;
using BusinessCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusinessCRM
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Retrieve connection string
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // 2. Configure Entity Framework Core and SQL Server with connection retry resiliency
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
                }
            });

            // 3. Configure ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 4. Configure Application Cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // 5. Add MVC Controllers with Views
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 6. Safe Startup Database Migration & Seeding (Prevents HTTP Error 500.30 Crash)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_RUNASP_SERVER"))
                    {
                        logger.LogWarning(
                            "Database connection string is not yet configured with real production credentials. " +
                            "Skipping startup database migration and seeding. " +
                            "Please update appsettings.Production.json or runasp.net environment variables with your production SQL Server details.");
                    }
                    else
                    {
                        var context = services.GetRequiredService<AppDbContext>();

                        logger.LogInformation("Attempting to apply database migrations on startup...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully.");

                        logger.LogInformation("Seeding initial roles and administrator...");
                        await DbInitializer.SeedRolesAndSuperAdminAsync(services);
                        logger.LogInformation("Database seeding completed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    // Catch and log without crashing the host process
                    logger.LogError(ex,
                        "An error occurred while connecting to the database, applying migrations, or seeding on startup. " +
                        "The application process will continue running so diagnostic logs can be inspected. " +
                        "Please verify your connection string and SQL Server connectivity.");
                }
            }

            // 7. Configure HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Default Route: directly opens Account/Login
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            await app.RunAsync();
        }
    }
}
