using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Migrations;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

// Master DB (stores tenant info)
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDb")));

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

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();
// Tenant services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// Config binding
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection(nameof(TenantSettings)));

builder.Services.AddControllersWithViews();
// Time out for session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(50);
});

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];


var app = builder.Build();


var scope = app.Services.CreateScope();

//var dbService = scope.ServiceProvider.GetService<IDBInitializer>();
//dbService.Initialize();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthorization();
app.UseSession();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();