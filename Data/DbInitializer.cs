using Microsoft.AspNetCore.Identity;
using MovieHub.Models;

namespace MovieHub.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "User", "Moderator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@moviehub.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    FullName = "Administrator",
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "admin123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            var moderatorEmail = "moderator@moviehub.com";
            var moderatorUser = await userManager.FindByEmailAsync(moderatorEmail);

            if (moderatorUser == null)
            {
                var user = new ApplicationUser
                {
                    FullName = "Movie Moderator",
                    UserName = "moderator",
                    Email = moderatorEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "moder123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Moderator");
                }
            }
        }
    }
}