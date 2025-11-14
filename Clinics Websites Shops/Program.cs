using Clinics_Websites_Shops;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.DataAccess.Extensions;
using Clinics_Websites_Shops.Middlewares;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;
using Stripe;

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
/// To Detremine Current Tenant By Domain
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();

    var tenant = tenantService.GetCurrentTenant(httpContextAccessor.HttpContext);

    if (tenant != null)
    {
        options.UseSqlServer(tenant.ConnectionString);
    }
    else
    {
        // For design-time or fallback (migrations)
        options.UseSqlServer("Server=.;Database=MasterDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }
});

// Tenant-aware ApplicationDbContext
//builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
//{
//    var envService = serviceProvider.GetRequiredService<EnvironmentService>();
//    // This will be overridden by tenant-specific connection string in OnConfiguring
//    options.ConfigureDatabase(appConnectionString, databaseProvider);
//});

/// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

/// Email sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddTransient<IEmailSender, EmailSender>();
// Tenant services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// Tenant services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

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

/// Time out for session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(50);
});

/// Configure Stripe API key
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.Run();
