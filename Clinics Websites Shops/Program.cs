using Clinics_Websites_Shops;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.DataAccess.Extensions;
using Clinics_Websites_Shops.Middlewares;
using Clinics_Websites_Shops.Services;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Register EnvironmentService as singleton
builder.Services.AddSingleton<EnvironmentService>();

// Get environment service instance for configuration
var environmentService = new EnvironmentService();
var databaseProvider = environmentService.GetDatabaseProvider();
var masterConnectionString = environmentService.GetMasterConnectionString();
var appConnectionString = environmentService.GetConnectionString();

// Master DB (stores tenant info)
builder.Services.AddDbContext<MasterDbContext>((serviceProvider, options) =>
{
    var envService = serviceProvider.GetRequiredService<EnvironmentService>();
    options.ConfigureDatabase(masterConnectionString, databaseProvider);
});

// Tenant-aware ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var envService = serviceProvider.GetRequiredService<EnvironmentService>();
    // This will be overridden by tenant-specific connection string in OnConfiguring
    options.ConfigureDatabase(appConnectionString, databaseProvider);
});

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Tenant services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<TenantManager>();
builder.Services.AddScoped<IRepository<UserOTP>, Repository<UserOTP>>();


// Config binding
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection(nameof(TenantSettings)));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddControllersWithViews();
builder.Services.AddLocalization();

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Register JSON localization
builder.Services.AddSingleton<JsonLocalizationOptions>(new JsonLocalizationOptions { ResourcesPath = "Resources" });
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
builder.Services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

// Configure cultures from environment
var supportedCulturesString = environmentService.GetValue("SUPPORTED_LANGUAGES", "en,ar");
var supportedCultures = supportedCulturesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(c => c.Trim()).ToArray();
var defaultCulture = environmentService.GetValue("DEFAULT_LANGUAGE", "en");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(defaultCulture)
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);

    options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(defaultCulture)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// Make sure RouteDataRequestCultureProvider is first
localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new RouteDataRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

// Route with culture parameter for all areas
var cultureConstraint = $@"^({string.Join("|", supportedCultures)})$";
app.MapControllerRoute(
    name: "localized",
    pattern: "{culture}/{area=Customer}/{controller=Home}/{action=Index}/{id?}",
    constraints: new { culture = cultureConstraint });

app.MapControllerRoute(
    name: "localizedAdmin",
    pattern: "{culture}/{area=Admin}/{controller=Home}/{action=Index}/{id?}",
    constraints: new { culture = cultureConstraint });

// Default route without culture (falls back to default culture)
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

// Development endpoints
if (app.Environment.IsDevelopment())
{
    // Seed roles and permissions
    app.MapGet("/seed-data", async (
        MasterDbContext masterDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager) =>
    {
        try
        {
            await DataSeeder.SeedAllData(masterDb, appDb, userManager, roleManager);
            return Results.Ok(new { success = true, message = "Data seeded successfully! Check console for details." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error seeding data: {ex.Message}");
            return Results.BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    });

    // Create test tenant and admin
    app.MapGet("/setup-test-tenant", async (
        MasterDbContext masterDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager) =>
    {
        try
        {
            Console.WriteLine("=== Creating Test Tenant and Admin User ===");

            // Step 1: Check if tenant already exists
            var existingTenant = await masterDb.Tenants
                .FirstOrDefaultAsync(t => t.Domain == "test.localhost");

            string tenantId;

            if (existingTenant != null)
            {
                Console.WriteLine($"Tenant already exists with ID: {existingTenant.TId}");
                tenantId = existingTenant.TId;
            }
            else
            {
                // Create new tenant
                tenantId = Guid.NewGuid().ToString();

                var tenant = new Tenant
                {
                    TId = tenantId,
                    Name = "Test Clinic",
                    Domain = "test.localhost",
                    ConnectionString = "Server=localhost;Port=3306;Database=ClinicsWebsiteShops;User=root;Password=;",
                    Status = true,
                    Locals = "en,ar",
                    CreatedAt = DateTime.UtcNow
                };

                await masterDb.Tenants.AddAsync(tenant);
                await masterDb.SaveChangesAsync();

                Console.WriteLine($"✓ Tenant created successfully with ID: {tenantId}");
            }

            // Step 2: Ensure SuperAdmin role exists
            var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole == null)
            {
                superAdminRole = new ApplicationRole
                {
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "Super Administrator with full system access",
                    IsSystemRole = true
                };

                var roleResult = await roleManager.CreateAsync(superAdminRole);
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("✓ SuperAdmin role created");
                }
                else
                {
                    Console.WriteLine("✗ Failed to create SuperAdmin role");
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Results.BadRequest(new { error = "Failed to create SuperAdmin role", details = errors });
                }
            }

            // Step 3: Check if admin user already exists
            var existingUser = await userManager.FindByEmailAsync("test@test.com");

            if (existingUser != null)
            {
                Console.WriteLine($"User already exists: {existingUser.Email}");

                // Update tenant ID if needed
                if (existingUser.TenantId != tenantId)
                {
                    existingUser.TenantId = tenantId;
                    await userManager.UpdateAsync(existingUser);
                    Console.WriteLine("✓ User tenant ID updated");
                }

                // Ensure user has SuperAdmin role
                if (!await userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(existingUser, "SuperAdmin");
                    Console.WriteLine("✓ SuperAdmin role assigned to user");
                }
            }
            else
            {
                // Create new admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "test@test.com",
                    Email = "test@test.com",
                    Name = "Test Admin",
                    Description = "Test Administrator",
                    Rate = 0,
                    TenantId = tenantId,
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(adminUser, "123456");

                if (userResult.Succeeded)
                {
                    Console.WriteLine("✓ Admin user created successfully");

                    // Assign SuperAdmin role
                    var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    if (roleAssignResult.Succeeded)
                    {
                        Console.WriteLine("✓ SuperAdmin role assigned to user");
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to assign SuperAdmin role");
                        var errors = string.Join(", ", roleAssignResult.Errors.Select(e => e.Description));
                        return Results.BadRequest(new { error = "Failed to assign SuperAdmin role", details = errors });
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to create admin user");
                    var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                    return Results.BadRequest(new { error = "Failed to create admin user", details = errors });
                }
            }

            var summary = new
            {
                success = true,
                message = "Test tenant and admin user created successfully!",
                tenantId = tenantId,
                tenantName = "Test Clinic",
                domain = "test.localhost",
                adminEmail = "test@test.com",
                adminPassword = "123456",
                loginUrl = "http://test.localhost:5292/Identity/Account/Login",
                note = "Make sure to add 'test.localhost' to your hosts file pointing to 127.0.0.1"
            };

            Console.WriteLine("\n=== Summary ===");
            Console.WriteLine($"Tenant ID: {tenantId}");
            Console.WriteLine($"Tenant Name: Test Clinic");
            Console.WriteLine($"Domain: test.localhost");
            Console.WriteLine($"Admin Email: test@test.com");
            Console.WriteLine($"Admin Password: 123456");
            Console.WriteLine("\nYou can now login at: http://test.localhost:5292/Identity/Account/Login");
            Console.WriteLine("(Make sure to add 'test.localhost' to your hosts file)");

            return Results.Ok(summary);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return Results.BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    });
}

app.Run();


