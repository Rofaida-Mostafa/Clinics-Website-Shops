using Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Clinics_Websites_Shops.Areas.CustomerIdentity.Controllers
{
    [Area(SD.CustomerIdentityArea)]
    public class AccountController : Controller
   {
       private readonly UserManager<ApplicationUser> _userManager;
       private readonly IEmailSender _emailSender;
       private readonly SignInManager<ApplicationUser> _signInManager;
       private readonly IHttpContextAccessor _httpContextAccessor;
       private readonly TenantDbContextFactory _tenantDbContextFactory;
	   //private readonly IRepository<UserOTP> _userOTP;


		public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
        SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor,
        TenantDbContextFactory tenantDbContextFactory)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _tenantDbContextFactory = tenantDbContextFactory;
			//_userOTP = userOTP;
        }


        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });

            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var user = new ApplicationUser()
            {
                Name = registerVM.Name,
                UserName = registerVM.UserName,
                Email = registerVM.Email,
            };

            // 1) Tenant for this domain
            if (!(HttpContext.Items["Tenant"] is Tenant tenant))
            {
                ModelState.AddModelError("", "Tenant not found for this domain!");
                return View(registerVM);
            }

            if (tenant == null)
            {
                ModelState.AddModelError("", "Tenant not found for this domain!");
                return View(registerVM);
            }

            // 2) DbContext for this tenant only
            var tenantContext = _tenantDbContextFactory.CreateDbContext();

            // 3) Identity stores
            var userStore = new UserStore<ApplicationUser>(tenantContext);
            var roleStore = new RoleStore<IdentityRole>(tenantContext);

			// 4) Tenant UserManager
			var tenantUserManager = new UserManager<ApplicationUser>(
	            userStore,
	            Options.Create(new IdentityOptions()),
	            new PasswordHasher<ApplicationUser>(),
	            new IUserValidator<ApplicationUser>[0],
	            new IPasswordValidator<ApplicationUser>[0],
	            new UpperInvariantLookupNormalizer(),
	            new IdentityErrorDescriber(),
	            _httpContextAccessor.HttpContext?.RequestServices,
				new LoggerFactory().CreateLogger<UserManager<ApplicationUser>>()


			);

            // ❗ استخدمي tenantUserManager بدل _userManager
            var result = await tenantUserManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(registerVM);
            }

            await tenantUserManager.AddToRoleAsync(user, SD.CustomerRole);


            //Send Email Confirmantion

            var token = await tenantUserManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action("ConfirmEmail", "Account", new
            {
                area = "CustomerIdentity",
                token,
                userId = user.Id
            }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm Your Email",
                $"<h1>Welcome to ClinicName</h1><p>Please confirm by <a href='{link}'>Clicking here</a></p>"
            );

            TempData["success-notification"] = "User sign up successfuly, Confirm your email!";
			return RedirectToAction("Login");
		}

        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {
			var user = await _userManager.FindByIdAsync(userId);
			if (user is null)
			{
				return NotFound();
			}
			var result = await _userManager.ConfirmEmailAsync(user, token);

			if (!result.Succeeded)
			{
				TempData["error-notification"] = "Invalid Token ,Resend Email Confirmation";
			}
			else
			{
				TempData["success-notification"] = "Activate Account Successfully";
			}
			return RedirectToAction("Login");
		}

		[HttpGet]
		public IActionResult Login()
		{
			if (User.Identity is not null && User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home", new { area = "Customer" });
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginVM loginVM)
		{
			if (!ModelState.IsValid)
			{
				return View(loginVM);
			}

			var user = await _userManager.FindByNameAsync(loginVM.EmailORUserName) ??
				await _userManager.FindByEmailAsync(loginVM.EmailORUserName);


			if (user is null)
			{
				TempData["error-notification"] = "Invalid User Name/Email Or Password!";
				return View(loginVM);
			}

			var result = await _userManager.CheckPasswordAsync(user, loginVM.Password);
			if (!result)
			{
				TempData["error-notification"] = "Invalid User Name/Email Or Password!";
				return View(loginVM);
			}

			if (!user.EmailConfirmed)
			{
				TempData["error-notification"] = "Please Confirm Your Account!";
				return View(loginVM);

			}

			if (!user.LockoutEnabled)
			{
				TempData["error-notification"] = $"You have a block till {user.LockoutEnd}";
				return View(loginVM);
			}

			await _signInManager.SignInAsync(user, loginVM.RememberMe);

			TempData["success-notification"] = "Login Successfully";

			return RedirectToAction("Index", "Home", new { area = "Customer" });

		}

		public IActionResult LogOut()
		{
			_signInManager.SignOutAsync();
			TempData["Success-notification"] = "Logout Successfully";
			return RedirectToAction("Login", "Account", new { area = "CustomerIdentity" });
		}

		[HttpGet]
		public IActionResult ResendEmailConfirmation()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ResendEmailConfirmationAsync(ResendEmailConfirmationVM resendEmailConfirmationVM)
		{
			if (!ModelState.IsValid)
			{
				return View(resendEmailConfirmationVM);

			}

			var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.EmailORUserName) ??
				await _userManager.FindByEmailAsync(resendEmailConfirmationVM.EmailORUserName);


			if (user is null)
			{
				TempData["error-notification"] = "Invalid User Name/Email!";
				return View(resendEmailConfirmationVM);
			}

			//Send Email Confirmation
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", UserId = user.Id, Token = token }, Request.Scheme);
			await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account!", $"<h1>Confirm Your Account By Clicking <a href='{link}'>here</a></h1>");

			TempData["success-notification"] = "Send Email Successfully,Please Confirm Your Account";
			return RedirectToAction("Login");
		}

		[HttpGet]
		public IActionResult ForgetPassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
		{
			if (!ModelState.IsValid)
			{
				return View(forgetPasswordVM);

			}

			var user = await _userManager.FindByNameAsync(forgetPasswordVM.EmailORUserName) ??
				await _userManager.FindByEmailAsync(forgetPasswordVM.EmailORUserName);


			if (user is null)
			{
				TempData["error-notification"] = "Invalid User Name/Email!";
				return View(forgetPasswordVM);
			}

			//Send Email Confirmation
			//var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var OTPNumber = new Random().Next(1000, 9999);


			await _emailSender.SendEmailAsync(user.Email!, "Reset Your Password!",
				$"Use this OTP Number: <b>{OTPNumber}</b> to reset your password. Don't Share it.");

			//await _userOTP.CreateAsync(new UserOTP()
			//{
			//	ApplicationUserId = user.Id,
			//	OTPNumber = OTPNumber.ToString(),
			//	ValidTo = DateTime.UtcNow.AddDays(1)
			//});
			//await _userOTP.CommitAsync();

			TempData["success-notification"] = "Send OTP to your Email Successfully,Please Check Your Email";
			return RedirectToAction("ConfirmOTP", "Account", new { area = "Identity", userId = user.Id });
		}

		[HttpGet]
		public IActionResult ConfirmOTP(string userId)
		{
			return View(new ConfirmOTPVM()
			{
				ApplicationUserId = userId
			});
		}
		[HttpPost]
		//public async Task<IActionResult> ConfirmOTP(ConfirmOTPVM confirmOTPVM)
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		return View(confirmOTPVM);

		//	}
		//	var user = await _userManager.FindByIdAsync(confirmOTPVM.ApplicationUserId);

		//	if (user is null)
		//	{
		//		return NotFound();
		//	}

		//	//var lastOTP = (await _userOTP.GetAsync(e => e.ApplicationUserId ==
		//	confirmOTPVM.ApplicationUserId)).OrderBy(e => e.Id).LastOrDefault();

		//	if (lastOTP is null)
		//	{
		//		return NotFound();
		//	}
		//	if (lastOTP.OTPNumber == confirmOTPVM.OTPNumber && lastOTP.ValidTo > DateTime.UtcNow)
		//	{
		//		return RedirectToAction("NewPassword", "Account", new
		//		{
		//			area = "Identity",
		//			userId = user.Id
		//		});
		//	}
		//	TempData["error-notification"] = "Invalid OTP Number!";
		//	return RedirectToAction("ConfirmOTP", "Account", new
		//	{
		//		area = "Identity",
		//		userId = confirmOTPVM.ApplicationUserId
		//	});
		//}

		[HttpGet]
		public IActionResult NewPassword(string userId)
		{
			return View(new NewPasswordVM()
			{
				ApplicationUserId = userId
			});
		}

		[HttpPost]
		public async Task<IActionResult> NewPasswordAsync(NewPasswordVM newPasswordVM)
		{
			if (!ModelState.IsValid)
			{
				return View(newPasswordVM);

			}
			var user = await _userManager.FindByIdAsync(newPasswordVM.ApplicationUserId);
			if (user is null)
			{
				return NotFound();

			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var result = await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

			if (!result.Succeeded)
			{
				foreach (var item in result.Errors)
				{
					ModelState.AddModelError(string.Empty, item.Description);
				}
				return View(newPasswordVM);
			}
			TempData["success-notification"] = "Reset Password Successfully";
			return RedirectToAction("Login");



		}
	}
}
