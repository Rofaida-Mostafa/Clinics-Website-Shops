using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Attributes;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LocationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Permission("Settings.Manage")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var locations = await _context.ClinicLocations
                .Where(l => l.TenantId == currentUser.TenantId)
                .OrderByDescending(l => l.IsDefault)
                .ThenBy(l => l.LocationName)
                .ToListAsync();

            return View(locations);
        }

        [HttpGet]
        [Permission("Settings.Manage")]
        public IActionResult Create()
        {
            return View(new ClinicLocation());
        }

        [HttpPost]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> Create(ClinicLocation model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // If this is set as default, unset all other defaults
            if (model.IsDefault)
            {
                var existingDefaults = await _context.ClinicLocations
                    .Where(l => l.TenantId == currentUser.TenantId && l.IsDefault)
                    .ToListAsync();

                foreach (var location in existingDefaults)
                {
                    location.IsDefault = false;
                }
            }

            model.TenantId = currentUser.TenantId;
            model.CreatedBy = currentUser.Id;
            model.CreatedAt = DateTime.UtcNow;

            _context.ClinicLocations.Add(model);
            await _context.SaveChangesAsync();

            TempData["success"] = "Location created successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var location = await _context.ClinicLocations
                .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == currentUser.TenantId);

            if (location == null)
            {
                TempData["error"] = "Location not found";
                return RedirectToAction(nameof(Index));
            }

            return View(location);
        }

        [HttpPost]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> Edit(ClinicLocation model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var location = await _context.ClinicLocations
                .FirstOrDefaultAsync(l => l.Id == model.Id && l.TenantId == currentUser.TenantId);

            if (location == null)
            {
                TempData["error"] = "Location not found";
                return RedirectToAction(nameof(Index));
            }

            // If this is set as default, unset all other defaults
            if (model.IsDefault && !location.IsDefault)
            {
                var existingDefaults = await _context.ClinicLocations
                    .Where(l => l.TenantId == currentUser.TenantId && l.IsDefault && l.Id != model.Id)
                    .ToListAsync();

                foreach (var loc in existingDefaults)
                {
                    loc.IsDefault = false;
                }
            }

            // Update location
            location.LocationName = model.LocationName;
            location.LocationCode = model.LocationCode;
            location.Description = model.Description;
            location.Address = model.Address;
            location.Area = model.Area;
            location.City = model.City;
            location.State = model.State;
            location.PostalCode = model.PostalCode;
            location.Country = model.Country;
            location.PrimaryPhone = model.PrimaryPhone;
            location.SecondaryPhone = model.SecondaryPhone;
            location.Fax = model.Fax;
            location.Email = model.Email;
            location.AlternativeEmail = model.AlternativeEmail;
            location.Website = model.Website;
            location.Latitude = model.Latitude;
            location.Longitude = model.Longitude;
            location.MapEmbedUrl = model.MapEmbedUrl;
            location.DirectionsUrl = model.DirectionsUrl;
            location.BusinessHours = model.BusinessHours;
            location.TimeZone = model.TimeZone;
            location.IsDefault = model.IsDefault;
            location.IsActive = model.IsActive;
            location.AcceptsAppointments = model.AcceptsAppointments;
            location.AcceptsWalkIns = model.AcceptsWalkIns;
            location.HasEmergencyServices = model.HasEmergencyServices;
            location.HasPharmacy = model.HasPharmacy;
            location.HasLaboratory = model.HasLaboratory;
            location.HasRadiology = model.HasRadiology;
            location.NumberOfRooms = model.NumberOfRooms;
            location.NumberOfBeds = model.NumberOfBeds;
            location.ParkingCapacity = model.ParkingCapacity;
            location.Facilities = model.Facilities;
            location.ServicesOffered = model.ServicesOffered;
            location.SpecialNotes = model.SpecialNotes;
            location.ImageUrl = model.ImageUrl;
            location.LastModified = DateTime.UtcNow;
            location.ModifiedBy = currentUser.Id;

            await _context.SaveChangesAsync();

            TempData["success"] = "Location updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var location = await _context.ClinicLocations
                .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == currentUser.TenantId);

            if (location == null)
            {
                TempData["error"] = "Location not found";
                return RedirectToAction(nameof(Index));
            }

            return View(location);
        }

        [HttpPost]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var location = await _context.ClinicLocations
                .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == currentUser.TenantId);

            if (location == null)
            {
                return Json(new { success = false, message = "Location not found" });
            }

            // Unset all other defaults
            var existingDefaults = await _context.ClinicLocations
                .Where(l => l.TenantId == currentUser.TenantId && l.IsDefault)
                .ToListAsync();

            foreach (var loc in existingDefaults)
            {
                loc.IsDefault = false;
            }

            // Set this as default
            location.IsDefault = true;
            location.LastModified = DateTime.UtcNow;
            location.ModifiedBy = currentUser.Id;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Default location updated successfully" });
        }

        [HttpPost]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var location = await _context.ClinicLocations
                .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == currentUser.TenantId);

            if (location == null)
            {
                return Json(new { success = false, message = "Location not found" });
            }

            // Don't allow deleting the default location
            if (location.IsDefault)
            {
                return Json(new { success = false, message = "Cannot delete the default location. Please set another location as default first." });
            }

            _context.ClinicLocations.Remove(location);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Location deleted successfully" });
        }
    }
}


