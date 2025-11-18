using Clinics_Websites_Shops.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class TenantServiceProviderFactory
{
    public static ServiceProvider BuildTenantProvider(string connectionString)
    {
        var services = new ServiceCollection();

        // Tenant DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Identity on TenantDb
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Logging
        services.AddLogging();

        // Email sender
        services.AddTransient<IEmailSender, EmailSender>();

        return services.BuildServiceProvider();
    }
}
