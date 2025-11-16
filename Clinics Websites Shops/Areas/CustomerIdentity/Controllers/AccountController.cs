//using Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel;
//using Clinics_Websites_Shops.Models;
//using Clinics_Websites_Shops.Services.IServices;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.AspNetCore.Identity.UI.Services;

//namespace Clinics_Websites_Shops.Areas.CustomerIdentity.Controllers
//{
//    [Area(SD.IdentityArea)]
//    public class AccountController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly IEmailSender _emailSender;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly TenantDbContextFactory _tenantDbContextFactory;


//        AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
//            SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor,
//           TenantDbContextFactory tenantDbContextFactory)
//        {
//            _userManager = userManager;
//            _emailSender = emailSender;
//            _signInManager = signInManager;
//            _httpContextAccessor = httpContextAccessor;
//            _tenantDbContextFactory = tenantDbContextFactory;

//        }


//        [HttpGet]
//        public IActionResult Register()
//        {
//            if (User.Identity!.IsAuthenticated)
//            {
//                return RedirectToAction("Index", "Home", new { area = "Customer" });

//            }
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Register(RegisterVM registerVM)
//        {
//            if (!ModelState.IsValid) return View(registerVM);

//            var user = new ApplicationUser()
//            {
//                Name = registerVM.Name,
//                UserName = registerVM.UserName,
//                Email = registerVM.Email,
//            };

//            // 1) Tenant for this domain
//            if (!(HttpContext.Items["Tenant"] is Tenant tenant))
//            {
//                ModelState.AddModelError("", "Tenant not found for this domain!");
//                return View(registerVM);
//            }

//            if (tenant == null)
//            {
//                ModelState.AddModelError("", "Tenant not found for this domain!");
//                return View(registerVM);
//            }

//            // 2) DbContext for this tenant only
//            var tenantContext = _tenantDbContextFactory.CreateDbContext();

//            // 3) Identity stores
//            var userStore = new UserStore<ApplicationUser>(tenantContext);
//            var roleStore = new RoleStore<IdentityRole>(tenantContext);

//            //// 4) Tenant UserManager
//            //var tenantUserManager = new UserManager<ApplicationUser>(
//            //    userStore,
//            //    null,
//            //    new PasswordHasher<ApplicationUser>(),
//            //    new IUserValidator<ApplicationUser>[0],
//            //    new IPasswordValidator<ApplicationUser>[0],
//            //    new UpperInvariantLookupNormalizer(),
//            //     new IdentityErrorDescriber(),
//            //    null
//            //    //_httpContextAccessor.HttpContext?.RequestServices


//            //);

//            // ❗ استخدمي tenantUserManager بدل _userManager
//            //var result = await tenantUserManager.CreateAsync(user, registerVM.Password);

//            if (!result.Succeeded)
//            {
//                foreach (var item in result.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, item.Description);
//                }
//                return View(registerVM);
//            }

//            await tenantUserManager.AddToRoleAsync(user, SD.CustomerRole);

//            var token = await tenantUserManager.GenerateEmailConfirmationTokenAsync(user);

//            var link = Url.Action("ConfirmEmail", "Account", new
//            {
//                area = "CustomerIdentity",
//                token,
//                userId = user.Id
//            }, Request.Scheme);

//            await _emailSender.SendEmailAsync(
//                user.Email,
//                "Confirm Your Email",
//                $"<h1>Welcome to ClinicName</h1><p>Please confirm by <a href='{link}'>Clicking here</a></p>"
//            );

//            TempData["success-notification"] = "User sign up successfuly, Confirm your email!";
//            return RedirectToAction("Index", "Home", new { area = "Customer" });
//        }

//        public async Task<IActionResult> ConfirmEmail(RegisterVM registerVM)
//        {

//            return View();
//        }
//    }
//}
