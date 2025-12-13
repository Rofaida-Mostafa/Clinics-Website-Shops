using Clinics_Websites_Shops.Areas.Tenance.ViewModel;
using Clinics_Websites_Shops.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.RegularExpressions;

public class TenantManager
{
    private readonly MasterDbContext _masterDb;
    private readonly IEmailSender _emailSender;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantManager> _logger;

    public TenantManager(MasterDbContext masterDb, IEmailSender emailSender, IServiceProvider serviceProvider, ILogger<TenantManager> logger)
    {
        _masterDb = masterDb;
        _emailSender = emailSender;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Tenant> CreateTenantAsync(RegisterVM model)
    {
        // Clean name 
        var cleanName = Regex.Replace(model.ClinicName.ToLower(), @"[^a-z0-9]", "");

        // Depending on Dynamic domain  
        string domain;
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Development")
        {
            // or Local environment
            domain = $"{cleanName}.localhost";
        }
        else
        {
            // Production environment
            domain = $"{cleanName}.com";
        }

        // Connection string
        var dbName = $"{cleanName}_Db";
        var connectionString = $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";

        // Create tenant
        var tenant = new Tenant
        {
            TId = Guid.NewGuid().ToString(),
            Name = model.ClinicName,
            Domain = domain,
            ConnectionString = connectionString,
            Locals = model.AvailableLocales != null && model.AvailableLocales.Any()
                     ? string.Join(",", model.AvailableLocales)
                     : "en"
        };

        _masterDb.Tenants.Add(tenant);
        await _masterDb.SaveChangesAsync();

        // Create tenant DB
        var tenantOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                   .UseSqlServer(connectionString)
                   .Options;

        using (var tenantDb = new ApplicationDbContext(tenantOptions))
        {
            await tenantDb.Database.EnsureCreatedAsync();
            await tenantDb.Database.MigrateAsync();
        }

        //Create Super Admin & send confirmation email
        await CreateTenantAdminAsync(tenant, model);

        return tenant;
    }

    private async Task CreateTenantAdminAsync(Tenant tenant, RegisterVM model)
    {
        //using var scope = _serviceProvider.CreateScope();
        using var scope = CreateTenantScope(tenant.ConnectionString);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // --- Create Admin Role if not exists ---
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        // --- Create Admin User ---
        var user = new ApplicationUser
        {
            Name = model.Name,
            UserName = model.UserName,
            Email = model.Email,
            TenantId = tenant.TId,
            EmailConfirmed = false
        };

       var result= await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError(errors);
            return;
        }
        await userManager.AddToRoleAsync(user, "Admin");

        // --- Generate Email Confirmation Token & Send Email ---
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
            $"https://{tenant.Domain}/Tenance/TenantAccount/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
            $"Click here: <a href='{confirmationLink}'>Confirm</a>");
    }

    private IServiceScope CreateTenantScope(string connectionString)
    {
        // 1) نعمل scope جديد من ال ServiceProvider
        var scope = _serviceProvider.CreateScope();

        // 2) ناخد الـ factory اللي بتسمح ننشئ DbContext ديناميكي
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        // 3) ننشئ DbContext مربوط بقاعدة التينانت
        var dbContext = factory.CreateDbContext();

        // نغير الـ Connection String بتاعته (وهي أهم خطوة)
        dbContext.Database.SetConnectionString(connectionString);

        return scope;
    }
}


