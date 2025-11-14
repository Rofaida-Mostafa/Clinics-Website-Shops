using Clinics_Websites_Shops.Areas.Tenance.ViewModel;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Clinics_Websites_Shops.Areas.Tenance.Controllers
{
    [Area("Tenance")]
    public class TenantAccountController : Controller
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly IEmailSender _emailSender;
        private readonly ITenantService _tenantService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantAccountController(
            MasterDbContext masterDbContext,
            IEmailSender emailSender,
            ITenantService tenantService,
            IHttpContextAccessor httpContextAccessor)
        {
            _masterDbContext = masterDbContext;
            _emailSender = emailSender;
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterVM());


        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Generate domain + DB
            var cleanClinicName = Regex.Replace(model.ClinicName.Trim().ToLower(), @"[^a-z0-9]", "");
            var domainName = $"{cleanClinicName}.local";
            var dbName = $"{cleanClinicName}_Db";
            var connectionString = $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";

            // Check if tenant already exists
            if (await _masterDbContext.Tenants.AnyAsync(t => t.Domain == domainName))
            {
                TempData["Error"] = "This clinic already exists!";
                return View(model);
            }

            // Add tenant to MasterDb
            var tenant = new Tenant
            {
                TId = Guid.NewGuid().ToString(),
                Name = model.ClinicName,
                Domain = domainName,
                ConnectionString = connectionString,
                Locals = model.AvailableLocales != null && model.AvailableLocales.Any() ? string.Join(",", model.AvailableLocales)
                : "en"
            };

            _masterDbContext.Tenants.Add(tenant);
            await _masterDbContext.SaveChangesAsync();

            try
            {
                // Create tenant database dynamically
                var tenantOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using (var tenantDb = new ApplicationDbContext(tenantOptions))
                {
                    await tenantDb.Database.EnsureCreatedAsync();
                    await tenantDb.Database.MigrateAsync();
                }

                // Setup Identity and create Super Admin
                var services = new ServiceCollection();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
                services.AddLogging();

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create Admin role if not exists
                if (!await roleManager.RoleExistsAsync("Admin"))
                    await roleManager.CreateAsync(new IdentityRole("Admin"));

                // Create Super Admin user
                var user = new ApplicationUser
                {
                    Name = model.Name,
                    UserName = model.UserName,
                    Email = model.Email,
                    TenantId = tenant.TId,
                    EmailConfirmed = false
                };

                var createResult = await userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return View(model);
                }

                await userManager.AddToRoleAsync(user, "Admin");

                // 6️⃣ Generate email confirmation token and link
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "TenantAccount",
                    new { userId = user.Id, token, domain = tenant.Domain }, Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"<h3>Welcome to {tenant.Name}!</h3>" +
                    $"<p>Click the link below to confirm your account:</p>" +
                    $"<a href='{confirmationLink}'>Confirm Email</a>");

                TempData["Success"] = "Clinic created successfully! Please check your email to confirm your account.";
                return RedirectToAction("Register");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error during setup: " + ex.Message;
                return View(model);
            }


        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                TempData["Error"] = "Invalid confirmation link.";
                return RedirectToAction("Register");
            }

            var tenant = await _masterDbContext.Tenants.FirstOrDefaultAsync(t => t.Domain == domain);
            if (tenant == null)
            {
                TempData["Error"] = "Tenant not found.";
                return RedirectToAction("Register");
            }

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(tenant.ConnectionString));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Register");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Email confirmation failed.";
                return RedirectToAction("Register");
            }

            // Redirect to login page (in the tenant domain)
            var loginUrl = $"https://{tenant.Domain}/Admin/Login";
            await _emailSender.SendEmailAsync(user.Email, "Your Account is Ready",
                $"<h3>Your account is confirmed!</h3><p>You can now log in:</p><a href='{loginUrl}'>Login</a>");

            TempData["Success"] = "Your email has been confirmed successfully!";
            return Redirect(loginUrl);
        }

    }
}
