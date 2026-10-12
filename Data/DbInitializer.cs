using BusinessCRM.Models;
using Microsoft.AspNetCore.Identity;

namespace BusinessCRM.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndSuperAdminAsync(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // 1. Seed Roles
                foreach (var roleName in UserRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation("Role created successfully: {RoleName}", roleName);
                        }
                    }
                }

                // 2. Seed Default Admin account (admin@businesscrm.com / Admin@123456)
                var adminEmail = "admin@businesscrm.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        FullName = "System Administrator",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var adminResult = await userManager.CreateAsync(adminUser, "Admin@123456");
                    if (adminResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                        await userManager.AddToRoleAsync(adminUser, UserRoles.SuperAdmin);
                        logger.LogInformation("Seeded default Admin: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to seed Admin user: {Errors}",
                            string.Join(", ", adminResult.Errors.Select(e => e.Description)));
                    }
                }

                // 3. Seed Default Employee account (employee@businesscrm.com / Employee@123456)
                var employeeEmail = "employee@businesscrm.com";
                var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
                if (employeeUser == null)
                {
                    employeeUser = new ApplicationUser
                    {
                        UserName = "employee",
                        Email = employeeEmail,
                        FullName = "Standard Employee",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var employeeResult = await userManager.CreateAsync(employeeUser, "Employee@123456");
                    if (employeeResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(employeeUser, UserRoles.Employee);
                        logger.LogInformation("Seeded default Employee: {Email}", employeeEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to seed Employee user: {Errors}",
                            string.Join(", ", employeeResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding roles and initial accounts.");
                throw;
            }
        }
    }
}
