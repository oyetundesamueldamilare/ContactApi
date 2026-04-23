using ContactApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ContactApi.Helpers
{
    public static class SeedDataHelper
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

    
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();

           

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            string[] roleNames = { "Admin", "Mentor", "Intern" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = configuration?["SeedAdmin:Email"];
            var adminPassword = configuration?["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = configuration?["SeedAdmin:FullName"] ?? "Admin",
                EmailConfirmed = true,
            };

            try
            {
                ApplicationUser? user = null;
                try
                {
                    user = await userManager.FindByEmailAsync(admin.Email);
                }
                catch
                {
                    try
                    {
                        var lookup = admin.Email.ToLowerInvariant();
                        user = await userManager.Users.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == lookup);
                    }
                    catch
                    {
                        user = null;
                    }
                }

                if (user == null)
                {
                    var result = await userManager.CreateAsync(admin, adminPassword);
                    if (result.Succeeded)
                    {
                        user = await userManager.FindByEmailAsync(admin.Email);
                    }
                }

                if (user != null)
                {
                    var rolesForUser = await userManager.GetRolesAsync(user);
                    if (rolesForUser == null || !rolesForUser.Contains("Admin"))
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
            catch
            {
            }
        }
    }

}
