using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class HomeController : Controller
    {
        // [Authorize(Roles = $"{SD.SuperAdminRole},{SD.AdminArea}")]
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }
        
        public IActionResult Index()
        {
            // Debug: Check current culture
            ViewBag.CurrentCulture = Thread.CurrentThread.CurrentCulture.Name;
            ViewBag.CurrentUICulture = Thread.CurrentThread.CurrentUICulture.Name;
            
            // Test localization directly
            var testTranslation = _localizer["patients"];
            ViewBag.TestTranslation = testTranslation.Value;
            
            return View();
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
