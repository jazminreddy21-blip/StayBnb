using Microsoft.AspNetCore.Identity;
using StayBnb.Models;

namespace StayBnb.Data
{
    public static class SuperAdminSeeder
    {
        public static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager)
        {
            string email = "superadmin@staybnb.com";
            string password = "StayBnb_Admin2026!";

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var superAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Staybnb Super Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    superAdmin, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        superAdmin, "SuperAdmin");
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }
            }
            else if (!await userManager.IsInRoleAsync(
                existingUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(
                    existingUser, "SuperAdmin");
            }
        }
    }
}