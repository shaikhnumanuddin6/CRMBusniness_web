
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

            // 1. Startup diagnostics
            var startupLogger = LoggerFactory
                .Create(logging => logging.AddConsole())
                .CreateLogger("StartupDiagnostics");

            startupLogger.LogInformation(
                "Starting BusinessCRM application.");

            startupLogger.LogInformation(
                "Environment: {Environment}",
                builder.Environment.EnvironmentName);

            // 2. Retrieve and validate the connection string
            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found. " +
                    "Configure it in appsettings.json, " +
                    "appsettings.Production.json, or environment variables.");
            }

            startupLogger.LogInformation(
                "DefaultConnection is configured.");

            // 3. Configure Entity Framework Core and SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
            });

            // 4. Configure ASP.NET Core Identity
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;

                    options.User.RequireUniqueEmail = true;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(15);

                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 5. Configure authentication cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";

                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy =
                    CookieSecurePolicy.Always;
            });

            // 6. Configure MVC controllers and views
            builder.Services.AddControllersWithViews();

            // 7. Build the application
            var app = builder.Build();

            // 8. Apply migrations and seed initial data
            // Database errors are logged without being silently ignored.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var databaseLogger = services
                    .GetRequiredService<ILogger<Program>>();

                try
                {
                    var context =
                        services.GetRequiredService<AppDbContext>();

                    databaseLogger.LogInformation(
                        "Checking database connectivity...");

                    var canConnect =
                        await context.Database.CanConnectAsync();

                    if (!canConnect)
                    {
                        databaseLogger.LogError(
                            "Cannot connect to the configured database. " +
                            "Check the production connection string and permissions.");
                    }
                    else
                    {
                        databaseLogger.LogInformation(
                            "Database connection successful.");

                        databaseLogger.LogInformation(
                            "Applying database migrations...");

                        await context.Database.MigrateAsync();

                        databaseLogger.LogInformation(
                            "Database migrations completed.");

                        databaseLogger.LogInformation(
                            "Seeding initial roles and administrator...");

                        await DbInitializer
                            .SeedRolesAndSuperAdminAsync(services);

                        databaseLogger.LogInformation(
                            "Database seeding completed.");
                    }
                }
                catch (Exception ex)
                {
                    databaseLogger.LogError(
                        ex,
                        "Database initialization failed. " +
                        "Check the database connection, permissions, " +
                        "migrations, and seed data.");
                }
            }

            // 9. Configure the HTTP request pipeline
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

            // 10. Default route opens the login page
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            // 11. Start the application
            startupLogger.LogInformation(
                "BusinessCRM startup pipeline configured.");

            await app.RunAsync();
        }
    }
}
