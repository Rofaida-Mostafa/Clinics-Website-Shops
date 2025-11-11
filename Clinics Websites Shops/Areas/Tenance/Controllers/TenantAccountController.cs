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
        public async Task<IActionResult> Register(RegisterVM registerVM)
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
            var applicationUser = new ApplicationUser
            {
                Name = registerVM.Name,
                UserName = registerVM.Email,
                Email = registerVM.Email,
                TenantId = tenant.TId
            };

            // create the tenant database + run migrations
            var tenantOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString).Options;
            try
            {
                using (var tenantDb = new ApplicationDbContext(tenantOptions))
                {
                    // Apply pending migrations dynamically
                    var pending = await tenantDb.Database.GetPendingMigrationsAsync();
                    if (pending.Any())
                    {
                        await tenantDb.Database.MigrateAsync();
                    }

                    // Setup Identity inside tenant DB via temporary service provider
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

                    // Check if user already exists
                    var existingUser = await userManager.FindByEmailAsync(registerVM.Email);
                    if (existingUser == null)
                    {
                        var superAdmin = new ApplicationUser
                        {
                            Name = registerVM.Name,
                            UserName = registerVM.UserName,
                            Email = registerVM.Email,
                            EmailConfirmed = true,
                            TenantId = tenant.TId
                        };

                        var createResult = await userManager.CreateAsync(superAdmin, registerVM.Password);

                        if (createResult.Succeeded)
                        {
                            // Generate email confirmation token
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                            var confirmUrl = Url.Action("ConfirmEmail", "TenantAccount",
                                new { userId = applicationUser.Id, token = token, domain = tenant.Domain },
                                protocol: Request.Scheme);

                            // Send confirmation email
                            await _emailSender.SendEmailAsync(applicationUser.Email, "Confirm your account",
                                $"<h2>Welcome {registerVM.Name}!</h2><p>Please confirm your account by clicking <a href='{confirmUrl}'>here</a>.</p>");

                            TempData["Info"] = "Registration successful! Please check your email to confirm your account.";

                        
                        }

                       if (!createResult.Succeeded)
                                {
                            foreach (var error in createResult.Errors)
                                ModelState.AddModelError("", error.Description);
                        }
         

             return View("RegisterConfirmation");
  
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "An error occurred while setting up your clinic. Please try again.";
                return RedirectToAction("Register", "Tenance");
            }

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


    }
}





