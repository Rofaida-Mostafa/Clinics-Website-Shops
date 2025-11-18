using System.Diagnostics;
using Clinics_Websites_Shops.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Clinics_Websites_Shops.Areas.Customer.ViewModels;
using Clinics_Websites_Shops.DataAccess;
using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Settings;

namespace Clinics_Websites_Shops.Areas.Customer.Controllers
{
    [Area(SD.CustomerArea)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly MasterDbContext _masterDb;

        public HomeController(
            ILogger<HomeController> logger,
            IStringLocalizer<HomeController> localizer,
            ApplicationDbContext context,
            ITenantService tenantService,
            MasterDbContext masterDb)
        {
            _logger = logger;
            _localizer = localizer;
            _context = context;
            _tenantService = tenantService;
            _masterDb = masterDb;
        }

        /// <summary>
        /// Get current tenant based on domain name from request
        /// </summary>
        private Tenant? GetCurrentTenant()
        {
            return _tenantService.GetCurrentTenant(HttpContext);
        }

        /// <summary>
        /// Get clinic settings for current tenant
        /// </summary>
        private async Task<ClinicSettings?> GetClinicSettingsAsync()
        {
            return await _context.ClinicSettings.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get default location for current tenant
        /// </summary>
        private async Task<ClinicLocation?> GetDefaultLocationAsync()
        {
            return await _context.ClinicLocations
                .FirstOrDefaultAsync(l => l.IsDefault && l.IsActive);
        }

        /// <summary>
        /// Get all active locations for current tenant
        /// </summary>
        private async Task<List<ClinicLocation>> GetActiveLocationsAsync()
        {
            return await _context.ClinicLocations
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.IsDefault)
                .ThenBy(l => l.LocationName)
                .ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
                ViewBag.TenantDomain = tenant.Domain;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.ClinicDescription = settings.ClinicDescription;
                ViewBag.LogoUrl = settings.LogoUrl;
                ViewBag.Phone = settings.Phone;
                ViewBag.Email = settings.Email;
                ViewBag.Address = settings.Address;
                ViewBag.City = settings.City;
                ViewBag.Country = settings.Country;
                ViewBag.BusinessHours = settings.BusinessHours;
                ViewBag.AllowOnlineBooking = settings.AllowOnlineBooking;
            }

            // Get default location
            var defaultLocation = await GetDefaultLocationAsync();
            if (defaultLocation != null)
            {
                ViewBag.DefaultLocation = defaultLocation;
            }

            // Get all active locations
            var locations = await GetActiveLocationsAsync();
            ViewBag.Locations = locations;

            return View();
        }


        public async Task<ViewResult> Doctors(DoctorFilterVM doctorFilterVM, int page = 1)
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.LogoUrl = settings.LogoUrl;
            }

            var doctors = _context.Doctors
                .Include(d => d.ApplicationUser)
                .Include(d => d.Department)
                .Where(d => d.IsActive)
                .AsQueryable();

            // Filter by department if specified
            if (doctorFilterVM.CategoryId is not null)
            {
                doctors = doctors.Where(e => e.DepartmentId == doctorFilterVM.CategoryId);
                ViewBag.CategoryId = doctorFilterVM.CategoryId;
            }

            double totalPages = Math.Ceiling(doctors.Count() / 8.0);
            int currentPage = page;

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;

            doctors = doctors.Skip((page -1) * 8).Take(8);

            // Get departments for filter
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = departments;

            return View(await doctors.ToListAsync());
        }


        public async Task<IActionResult> Specialties()
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.LogoUrl = settings.LogoUrl;
            }

            // Get all departments/specialties
            var departments = await _context.Departments
                .Include(d => d.Translations)
                .ToListAsync();

            ViewBag.Departments = departments;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Appointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Doctor([FromRoute] int id)
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.LogoUrl = settings.LogoUrl;
            }

            var doctor = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .Include(d => d.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (doctor is null)
                return NotFound();

            // Get doctor's schedules
            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == id && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync();
            ViewBag.Schedules = schedules;

            // Get doctor's upcoming holidays
            var today = DateTime.Today;
            var holidays = await _context.DoctorHolidays
                .Where(h => h.DoctorId == id && h.EndDate >= today && h.IsApproved)
                .OrderBy(h => h.StartDate)
                .Take(5)
                .ToListAsync();
            ViewBag.Holidays = holidays;

            return View(doctor);
        }

        public async Task<IActionResult> Privacy()
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.LogoUrl = settings.LogoUrl;
            }

            return View();
        }

        public async Task<IActionResult> About()
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.ClinicDescription = settings.ClinicDescription;
                ViewBag.LogoUrl = settings.LogoUrl;
                ViewBag.Phone = settings.Phone;
                ViewBag.Email = settings.Email;
                ViewBag.Address = settings.Address;
                ViewBag.City = settings.City;
                ViewBag.Country = settings.Country;
                ViewBag.BusinessHours = settings.BusinessHours;
            }

            // Get all active locations
            var locations = await GetActiveLocationsAsync();
            ViewBag.Locations = locations;

            return View();
        }

        public async Task<IActionResult> Contact()
        {
            // Get tenant information
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                ViewBag.TenantName = tenant.Name;
            }

            // Get clinic settings
            var settings = await GetClinicSettingsAsync();
            if (settings != null)
            {
                ViewBag.ClinicName = settings.ClinicName;
                ViewBag.LogoUrl = settings.LogoUrl;
                ViewBag.Phone = settings.Phone;
                ViewBag.Email = settings.Email;
                ViewBag.Address = settings.Address;
                ViewBag.City = settings.City;
                ViewBag.Country = settings.Country;
            }

            // Get default location
            var defaultLocation = await GetDefaultLocationAsync();
            if (defaultLocation != null)
            {
                ViewBag.DefaultLocation = defaultLocation;
            }

            // Get all active locations
            var locations = await GetActiveLocationsAsync();
            ViewBag.Locations = locations;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
