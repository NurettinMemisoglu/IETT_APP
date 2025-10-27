using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IETT_APP.Infrastructure.Persistence.Seed
{
    public class IdentitySeed
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "Admin";
            string fullName = "Admin";
            string email = "admin@gmail.com";
            string password = "Admin123*";

            if (await roleManager.FindByNameAsync(roleName) == null)
                await roleManager.CreateAsync(new IdentityRole(roleName));

            // Use email as username
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
        public static async Task SeedPlannerAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "Planner";
            string fullName = "Planner";
            string email = "Planner@gmail.com";
            string password = "Planner123*";

            if (await roleManager.FindByNameAsync(roleName) == null)
                await roleManager.CreateAsync(new IdentityRole(roleName));

            // Use email as username
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
        public static async Task SeedChiefAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "Chief";
            string fullName = "Chief";
            string email = "Chief@gmail.com";
            string password = "Chief123*";

            if (await roleManager.FindByNameAsync(roleName) == null)
                await roleManager.CreateAsync(new IdentityRole(roleName));

            // Use email as username
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
        public static async Task SeedDriverAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "Driver";
            string fullName = "Driver";
            string email = "Driver@gmail.com";
            string password = "Driver123*";

            if (await roleManager.FindByNameAsync(roleName) == null)
                await roleManager.CreateAsync(new IdentityRole(roleName));

            // Use email as username
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
        public static async Task SeedUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "User";
            string fullName = "User";
            string email = "User@gmail.com";
            string password = "User123*";

            if (await roleManager.FindByNameAsync(roleName) == null)
                await roleManager.CreateAsync(new IdentityRole(roleName));

            // Use email as username
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
