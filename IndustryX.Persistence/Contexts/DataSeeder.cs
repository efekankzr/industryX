using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryX.Persistence.Contexts
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = configuration.GetSection("Roles").GetChildren().Select(x => x.Value).ToList();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var adminSection = configuration.GetSection("AdminUser");
            var email = adminSection["Email"];
            var password = adminSection["Password"];
            var firstname = adminSection["Firstname"];
            var surname = adminSection["Surname"];
            var role = adminSection["Role"];

            var admin = await userManager.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Firstname = firstname,
                    Surname = surname,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded && await roleManager.RoleExistsAsync(role))
                {
                    await userManager.AddToRoleAsync(admin, role);
                }
            }
        }
    }
}
