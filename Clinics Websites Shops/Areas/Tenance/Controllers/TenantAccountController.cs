using Clinics_Websites_Shops.Areas.Tenance.ViewModel;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Clinics_Websites_Shops.Areas.Tenance.Controllers
{
    [Area(SD.TenanceArea)]
    public class TenantAccountController : Controller
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;


        public TenantAccountController(
            MasterDbContext masterDbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender, IConfiguration config)
        {
            _masterDbContext = masterDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }
        [HttpPost]
        public async Task<IActionResult> SaveRegister(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            // Validate at least one locale
            if (registerVM.AvailableLocales == null || !registerVM.AvailableLocales.Any())
            {
                ModelState.AddModelError("SelectedLocales", "Please select at least one locale.");
                return View(registerVM);
            }

            // Generate domain and database name
            var clinicNameClean = registerVM.ClinicName.Trim().Replace(" ", "").ToLower();
            var domainName = $"{clinicNameClean}.local";
            var dbName = $"{clinicNameClean}Db";
            var connectionString = $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";

            // Check if tenant already exists
            if (await _masterDbContext.Tenants.AnyAsync(t => t.Domain == domainName))
            {
                ModelState.AddModelError("", "This clinic already exists.");
                return View(registerVM);
            }

            // Create Tenant entry in Master DB
            var tenant = new Tenant
            {
                TId = Guid.NewGuid().ToString(),
                Name = registerVM.ClinicName,
                Domain = domainName,
                ConnectionString = connectionString,
                Locals = string.Join(",", registerVM.AvailableLocales),
                Status = true
            };

            _masterDbContext.Tenants.Add(tenant);
            await _masterDbContext.SaveChangesAsync();

            // Create user in Identity
            var user = new ApplicationUser
            {
                Name = registerVM.Name,
                UserName = registerVM.Email,
                Email = registerVM.Email,
                TenantId = tenant.TId
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (result.Succeeded)
            {
                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmUrl = Url.Action("ConfirmEmail", "TenantAccount",
                    new { userId = user.Id, token = token, domain = tenant.Domain },
                    protocol: Request.Scheme);

                // Send confirmation email
                await _emailSender.SendEmailAsync(user.Email, "Confirm your account",
                    $"<h2>Welcome {registerVM.Name}!</h2><p>Please confirm your account by clicking <a href='{confirmUrl}'>here</a>.</p>");

                TempData["Info"] = "Registration successful! Please check your email to confirm your account.";
                return View("RegisterConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(registerVM);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, string domain)
        {
            if (userId == null || token == null)
            {
                TempData["Error"] = "Invalid confirmation link.";
                return RedirectToAction("Register", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Register", "Account");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Email confirmation failed.";
                return RedirectToAction("Register", "Account");
            }

            // Get tenant from Master DB
            var tenant = await _masterDbContext.Tenants
                .FirstOrDefaultAsync(t => t.TId == user.TenantId);

            if (tenant == null)
            {
                TempData["Error"] = "Tenant not found.";
                return RedirectToAction("Register", "Account");
            }

            try
            {
                // Create tenant DB and apply migrations
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(tenant.ConnectionString);

                using (var tenantDb = new ApplicationDbContext(optionsBuilder.Options))
                {
                    await tenantDb.Database.MigrateAsync();

                    // Setup Identity in the tenant DB
                    var services = new ServiceCollection();
                    services.AddLogging();
                    services.AddIdentityCore<ApplicationUser>(options =>
                    {
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>();

                    services.AddSingleton(tenantDb);
                    using var serviceProvider = services.BuildServiceProvider();
                    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    // Add SuperAdmin user if not exists
                    var existingUser = await userManager.FindByEmailAsync(user.Email);
                    if (existingUser == null)
                    {
                        var superAdmin = new ApplicationUser
                        {
                            Name = user.Name,
                            UserName = user.UserName,
                            Email = user.Email,
                            EmailConfirmed = true,
                            TenantId = user.TenantId
                        };

                        var createResult = await userManager.CreateAsync(superAdmin, "Default@123");
                        if (!createResult.Succeeded)
                        {
                            foreach (var error in createResult.Errors)
                                Console.WriteLine(error.Description);
                        }
                    }
                }

                // Build new domain login URL
                var loginUrl = $"https://{tenant.Domain}/Admin/Login";

                // Send confirmation email with login link
                var message = $"<h3>Your account has been successfully confirmed!</h3>" +
                              $"<p>You can now log in using your domain:</p>" +
                              $"<a href='{loginUrl}'>{loginUrl}</a>";

                await _emailSender.SendEmailAsync(user.Email, "Your Account is Ready", message);


                TempData["Success"] = "Your email has been confirmed and your clinic is ready! Redirecting to login page...";
                return Redirect(loginUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "An error occurred while setting up your clinic. Please try again.";
                return RedirectToAction("Register", "Tenance");
            }
        }


    }
}





