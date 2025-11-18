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
    [Area(SD.TenanceArea)]
    public class TenantAccountController : Controller
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly IEmailSender _emailSender;
        private readonly ITenantService _tenantService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenantManager _tenantManager;

        public TenantAccountController(
            MasterDbContext masterDbContext,
            IEmailSender emailSender,
            ITenantService tenantService,
            IHttpContextAccessor httpContextAccessor,
            TenantManager tenantManager)
        {
            _masterDbContext = masterDbContext;
            _emailSender = emailSender;
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
            _tenantManager = tenantManager;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterVM());


        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var tenant = await _tenantManager.CreateTenantAsync(model);

               

                TempData["Success"] = "Clinic created! Check email for confirmation.";
                return RedirectToAction("Register");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string token, string domain)
        {
            var tenant = _tenantService.GetCurrentTenant(HttpContext)?? 
            await _masterDbContext.Tenants.FirstOrDefaultAsync(t => t.Domain == domain);

            if (tenant == null) return RedirectToAction("Register");

            using var scope = CreateTenantScope(tenant.ConnectionString);
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("Register");

            await userManager.ConfirmEmailAsync(user, token);
            return Redirect($"https://{tenant.Domain}/Admin/Login");
        }

        private IServiceScope CreateTenantScope(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(connectionString));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            services.AddLogging();
            var provider = services.BuildServiceProvider();
            return provider.CreateScope();
        }

    }
}
