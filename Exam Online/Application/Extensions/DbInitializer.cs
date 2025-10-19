using Exam_Online;
using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Extensions
{
    public static class DbInitializer
    {
        public static async Task ApplyMigrationsAsync(WebApplication app)
        {
            using (var Scope = app.Services.CreateScope())
            {
                var Services = Scope.ServiceProvider;
                var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();

                try
                {
                    var dbContext = Services.GetRequiredService<ApplicationDbContext>();

                    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
                    {
                        await dbContext.Database.MigrateAsync();
                    }

                }
                catch (Exception ex)
                {
                    var logger = LoggerFactory.CreateLogger<Program>();
                    logger.LogError(ex.Message, "An Error Occured During Appling Migrations");
                }
            }
        }
        public static async Task SeedRoles(WebApplication app)
        {
            using (var Scope = app.Services.CreateScope())
            {
                var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                
                string[] roles = { "User", "Admin", "SuperAdmin" };
                
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
        }

        public static async Task SeedUserAsync(WebApplication app)
        {

            using (var Scope = app.Services.CreateScope()) {
                var Services = Scope.ServiceProvider;
                var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();
                var logger = LoggerFactory.CreateLogger<Program>();


                try
                {
                    var userManager = Services.GetRequiredService<UserManager<User>>();

                    if (!userManager.Users.Any())
                    {
                        var User = new User
                        {
                            FirstName = "Islam",
                            LastName = "Klieb",
                            Email = "eslamklyep2019@gmail.com",
                            UserName = "islamklieb",
                            PhoneNumber = "1234567890",
                        };
                        var result = await userManager.CreateAsync(User, "@AanTk5A7V1W2h99Thm");

                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            logger.LogWarning("Failed to seed default user: {Errors}", errors);
                        }
                        else
                        {
                            await userManager.AddToRoleAsync(User, "SuperAdmin");
                            logger.LogInformation("Default user created successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the default user.");
                }

            }
        }

    }
}
