using Microsoft.AspNetCore.Identity;
using WhiteSoft.Models;

namespace PojistakNET.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // It create roles, if isn't exist
            string[] roleNames = { "superadmin", "admin", "user" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // It create user, if isn't exist
            var adminEmail = "admin@admin.cz";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FirstName = "Jan",
                    LastName = "Novák",
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "superadmin"); // nastav heslo
                await userManager.AddToRoleAsync(adminUser, "superadmin");
            }
        }
    }

}