using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;
using StayBnb.DataSeeders;

var builder = WebApplication.CreateBuilder(args);

// 1. Get the SQL Server connection string
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

// 2. Register the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 3. Configure Identity and roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 4. Register MVC controllers and views
builder.Services.AddControllersWithViews();

// Build the application
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services
        .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = services
        .GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedRolesAsync(roleManager);

    await SuperAdminSeeder.SeedSuperAdminAsync(userManager);

    // Get the database context
    var dbContext = services
        .GetRequiredService<ApplicationDbContext>();

    // Find the seeded SuperAdmin account
    var superAdmin = await userManager
        .FindByEmailAsync("superadmin@staybnb.com");

    // Add sample properties if the database is empty
    if (superAdmin != null)
    {
        await SamplePropertySeeder.SeedAsync(
            dbContext,
            superAdmin.Id);
    }

    // 5. Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    // Register Identity pages for Login and Register
    app.MapRazorPages()
        .WithStaticAssets();

    app.Run();
}