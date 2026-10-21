using Microsoft.AspNetCore.Identity;

namespace StayBnb.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "Guest",
                "Host",
                "Admin",
                "SuperAdmin"
            };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }
    }
}