using CampusRooms.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // the Identity UI (Register, Log in) is Razor Pages

// EF Core: any controller that asks for an AppDbContext gets one,
// connected to the database named in appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CampusRoomsDb")));

// Identity: accounts and roles, stored by AppDbContext. No email confirmation (the app can't send email).
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// CanManageRooms = Admin or Staff
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageRooms", policy => policy.RequireRole("Admin", "Staff"));
});

var app = builder.Build();

// Start-up: make the two roles if missing, and make the AdminEmail account an Admin (once it is registered).
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (string roleName in new[] { "Admin", "Staff" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        string? adminEmail = app.Configuration["AdminEmail"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            IdentityUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        // e.g. the database has not been made yet (run Update-Database)
        app.Logger.LogWarning(ex, "Could not seed roles. Has Update-Database been run?");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
