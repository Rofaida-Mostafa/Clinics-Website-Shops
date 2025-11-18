using Clinics_Websites_Shops.Areas.Identity.ViewModels;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Clinics_Websites_Shops.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly IStringLocalizer<AccountController> _localizer;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ITenantService tenantService,
            IStringLocalizer<AccountController> localizer,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tenantService = tenantService;
            _localizer = localizer;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Get current tenant
                var currentTenant = _tenantService.GetFirstTenant();
                if (currentTenant == null)
                {
                    ModelState.AddModelError(string.Empty, _localizer["tenantNotFound"].Value);
                    return View(viewModel);
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, _localizer["invalidLoginAttempt"].Value);
                    return View(viewModel);
                }

                // Check if user belongs to current tenant
                if (user.TenantId != currentTenant.TId)
                {
                    ModelState.AddModelError(string.Empty, _localizer["invalidLoginAttempt"].Value);
                    _logger.LogWarning("User {Email} attempted to login to wrong tenant", viewModel.Email);
                    return View(viewModel);
                }

                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    viewModel.Password,
                    viewModel.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", viewModel.Email);
                    
                    // Redirect to return URL or default admin page
                    if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
                    {
                        return Redirect(viewModel.ReturnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account locked out", viewModel.Email);
                    ModelState.AddModelError(string.Empty, _localizer["accountLockedOut"].Value);
                    return View(viewModel);
                }

                ModelState.AddModelError(string.Empty, _localizer["invalidLoginAttempt"].Value);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for {Email}", viewModel.Email);
                ModelState.AddModelError(string.Empty, _localizer["loginError"].Value);
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}

