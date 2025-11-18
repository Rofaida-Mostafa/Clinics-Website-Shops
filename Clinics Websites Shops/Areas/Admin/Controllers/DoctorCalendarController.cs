using Clinics_Websites_Shops.Attributes;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DoctorCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorCalendarController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/DoctorCalendar/Index/{doctorId}
        [Permission("Doctors.View")]
        public async Task<IActionResult> Index(int? doctorId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Get all doctors for dropdown
            ViewBag.Doctors = await _context.Doctors
                .Where(d => d.TenantId == currentUser.TenantId && d.IsActive)
                .Include(d => d.ApplicationUser)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.ApplicationUser!.Name} ({d.DoctorId})"
                })
                .ToListAsync();

            if (doctorId.HasValue)
            {
                ViewBag.SelectedDoctorId = doctorId.Value;

                // Get doctor details
                var doctor = await _context.Doctors
                    .Include(d => d.ApplicationUser)
                    .FirstOrDefaultAsync(d => d.Id == doctorId.Value && d.TenantId == currentUser.TenantId);

                if (doctor != null)
                {
                    ViewBag.DoctorName = doctor.ApplicationUser!.Name;
                }

                // Get schedules
                var schedules = await _context.DoctorSchedules
                    .Include(s => s.Location)
                    .Where(s => s.DoctorId == doctorId.Value && s.TenantId == currentUser.TenantId)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

                ViewBag.Schedules = schedules;

                // Get holidays
                var holidays = await _context.DoctorHolidays
                    .Include(h => h.ReplacementDoctor)
                    .ThenInclude(d => d!.ApplicationUser)
                    .Where(h => h.DoctorId == doctorId.Value && h.TenantId == currentUser.TenantId)
                    .OrderByDescending(h => h.StartDate)
                    .ToListAsync();

                ViewBag.Holidays = holidays;
            }

            return View();
        }

        // GET: Admin/DoctorCalendar/CreateSchedule/{doctorId}
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> CreateSchedule(int doctorId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var doctor = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.TenantId == currentUser.TenantId);

            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.DoctorId = doctorId;
            ViewBag.DoctorName = doctor.ApplicationUser!.Name;

            // Get locations for dropdown
            ViewBag.Locations = await _context.ClinicLocations
                .Where(l => l.TenantId == currentUser.TenantId && l.IsActive)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.LocationName
                })
                .ToListAsync();

            var schedule = new DoctorSchedule
            {
                DoctorId = doctorId,
                TenantId = currentUser.TenantId,
                StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                SlotDurationMinutes = 30,
                BufferTimeMinutes = 0,
                MaxAppointmentsPerSlot = 1,
                IsActive = true
            };

            return View(schedule);
        }

        // POST: Admin/DoctorCalendar/CreateSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> CreateSchedule(DoctorSchedule schedule)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Verify doctor belongs to tenant
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == schedule.DoctorId && d.TenantId == currentUser.TenantId);

            if (doctor == null)
            {
                return NotFound();
            }

            schedule.TenantId = currentUser.TenantId;
            schedule.CreatedAt = DateTime.UtcNow;
            schedule.CreatedBy = currentUser.Id;

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Schedule created successfully!";
            return RedirectToAction(nameof(Index), new { doctorId = schedule.DoctorId });
        }

        // GET: Admin/DoctorCalendar/EditSchedule/{id}
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> EditSchedule(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var schedule = await _context.DoctorSchedules
                .Include(s => s.Doctor)
                .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == currentUser.TenantId);

            if (schedule == null)
            {
                return NotFound();
            }

            ViewBag.DoctorName = schedule.Doctor!.ApplicationUser!.Name;

            // Get locations for dropdown
            ViewBag.Locations = await _context.ClinicLocations
                .Where(l => l.TenantId == currentUser.TenantId && l.IsActive)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.LocationName
                })
                .ToListAsync();

            return View(schedule);
        }

        // POST: Admin/DoctorCalendar/EditSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> EditSchedule(int id, DoctorSchedule schedule)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (id != schedule.Id)
            {
                return NotFound();
            }

            var existingSchedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == currentUser.TenantId);

            if (existingSchedule == null)
            {
                return NotFound();
            }

            existingSchedule.DayOfWeek = schedule.DayOfWeek;
            existingSchedule.StartTime = schedule.StartTime;
            existingSchedule.EndTime = schedule.EndTime;
            existingSchedule.BreakStartTime = schedule.BreakStartTime;
            existingSchedule.BreakEndTime = schedule.BreakEndTime;
            existingSchedule.SlotDurationMinutes = schedule.SlotDurationMinutes;
            existingSchedule.BufferTimeMinutes = schedule.BufferTimeMinutes;
            existingSchedule.MaxAppointmentsPerSlot = schedule.MaxAppointmentsPerSlot;
            existingSchedule.LocationId = schedule.LocationId;
            existingSchedule.RoomNumber = schedule.RoomNumber;
            existingSchedule.IsActive = schedule.IsActive;
            existingSchedule.Notes = schedule.Notes;
            existingSchedule.EffectiveFrom = schedule.EffectiveFrom;
            existingSchedule.EffectiveTo = schedule.EffectiveTo;
            existingSchedule.LastModified = DateTime.UtcNow;
            existingSchedule.ModifiedBy = currentUser.Id;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Schedule updated successfully!";
            return RedirectToAction(nameof(Index), new { doctorId = existingSchedule.DoctorId });
        }

        // POST: Admin/DoctorCalendar/DeleteSchedule/{id}
        [HttpPost]
        [Permission("Doctors.Delete")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == currentUser.TenantId);

            if (schedule == null)
            {
                return Json(new { success = false, message = "Schedule not found" });
            }

            var doctorId = schedule.DoctorId;
            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Schedule deleted successfully", doctorId });
        }

        // GET: Admin/DoctorCalendar/CreateHoliday/{doctorId}
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> CreateHoliday(int doctorId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var doctor = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.TenantId == currentUser.TenantId);

            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.DoctorId = doctorId;
            ViewBag.DoctorName = doctor.ApplicationUser!.Name;

            // Get other doctors for replacement dropdown
            ViewBag.ReplacementDoctors = await _context.Doctors
                .Where(d => d.TenantId == currentUser.TenantId && d.IsActive && d.Id != doctorId)
                .Include(d => d.ApplicationUser)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.ApplicationUser!.Name
                })
                .ToListAsync();

            var holiday = new DoctorHoliday
            {
                DoctorId = doctorId,
                TenantId = currentUser.TenantId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                IsFullDay = true,
                BlockNewAppointments = true,
                CancelExistingAppointments = false,
                IsApproved = true,
                Status = "Approved"
            };

            return View(holiday);
        }

        // POST: Admin/DoctorCalendar/CreateHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> CreateHoliday(DoctorHoliday holiday)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Verify doctor belongs to tenant
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == holiday.DoctorId && d.TenantId == currentUser.TenantId);

            if (doctor == null)
            {
                return NotFound();
            }

            // Validate dates
            if (holiday.EndDate < holiday.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
                return View(holiday);
            }

            holiday.TenantId = currentUser.TenantId;
            holiday.CreatedAt = DateTime.UtcNow;
            holiday.CreatedBy = currentUser.Id;

            if (holiday.IsApproved)
            {
                holiday.ApprovedAt = DateTime.UtcNow;
                holiday.ApprovedBy = currentUser.Id;
            }

            _context.DoctorHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Holiday created successfully!";
            return RedirectToAction(nameof(Index), new { doctorId = holiday.DoctorId });
        }

        // GET: Admin/DoctorCalendar/EditHoliday/{id}
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> EditHoliday(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var holiday = await _context.DoctorHolidays
                .Include(h => h.Doctor)
                .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(h => h.Id == id && h.TenantId == currentUser.TenantId);

            if (holiday == null)
            {
                return NotFound();
            }

            ViewBag.DoctorName = holiday.Doctor!.ApplicationUser!.Name;

            // Get other doctors for replacement dropdown
            ViewBag.ReplacementDoctors = await _context.Doctors
                .Where(d => d.TenantId == currentUser.TenantId && d.IsActive && d.Id != holiday.DoctorId)
                .Include(d => d.ApplicationUser)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.ApplicationUser!.Name
                })
                .ToListAsync();

            return View(holiday);
        }

        // POST: Admin/DoctorCalendar/EditHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Doctors.Edit")]
        public async Task<IActionResult> EditHoliday(int id, DoctorHoliday holiday)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (id != holiday.Id)
            {
                return NotFound();
            }

            var existingHoliday = await _context.DoctorHolidays
                .FirstOrDefaultAsync(h => h.Id == id && h.TenantId == currentUser.TenantId);

            if (existingHoliday == null)
            {
                return NotFound();
            }

            // Validate dates
            if (holiday.EndDate < holiday.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
                return View(holiday);
            }

            existingHoliday.Title = holiday.Title;
            existingHoliday.Description = holiday.Description;
            existingHoliday.StartDate = holiday.StartDate;
            existingHoliday.EndDate = holiday.EndDate;
            existingHoliday.StartTime = holiday.StartTime;
            existingHoliday.EndTime = holiday.EndTime;
            existingHoliday.HolidayType = holiday.HolidayType;
            existingHoliday.IsFullDay = holiday.IsFullDay;
            existingHoliday.IsRecurring = holiday.IsRecurring;
            existingHoliday.RecurrencePattern = holiday.RecurrencePattern;
            existingHoliday.RecurrenceDayOfWeek = holiday.RecurrenceDayOfWeek;
            existingHoliday.Status = holiday.Status;
            existingHoliday.IsApproved = holiday.IsApproved;
            existingHoliday.BlockNewAppointments = holiday.BlockNewAppointments;
            existingHoliday.CancelExistingAppointments = holiday.CancelExistingAppointments;
            existingHoliday.NotificationMessage = holiday.NotificationMessage;
            existingHoliday.ReplacementDoctorId = holiday.ReplacementDoctorId;
            existingHoliday.LastModified = DateTime.UtcNow;
            existingHoliday.ModifiedBy = currentUser.Id;

            if (holiday.IsApproved && !existingHoliday.IsApproved)
            {
                existingHoliday.ApprovedAt = DateTime.UtcNow;
                existingHoliday.ApprovedBy = currentUser.Id;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Holiday updated successfully!";
            return RedirectToAction(nameof(Index), new { doctorId = existingHoliday.DoctorId });
        }

        // POST: Admin/DoctorCalendar/DeleteHoliday/{id}
        [HttpPost]
        [Permission("Doctors.Delete")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var holiday = await _context.DoctorHolidays
                .FirstOrDefaultAsync(h => h.Id == id && h.TenantId == currentUser.TenantId);

            if (holiday == null)
            {
                return Json(new { success = false, message = "Holiday not found" });
            }

            var doctorId = holiday.DoctorId;
            _context.DoctorHolidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Holiday deleted successfully", doctorId });
        }

        // API: Check if doctor is available on a specific date/time
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int doctorId, DateTime date, TimeSpan time)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { available = false, message = "User not found" });
            }

            // Check if doctor has a schedule for this day
            var dayOfWeek = (int)date.DayOfWeek;
            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId
                    && s.TenantId == currentUser.TenantId
                    && s.DayOfWeek == dayOfWeek
                    && s.IsActive
                    && (!s.EffectiveFrom.HasValue || s.EffectiveFrom.Value <= date)
                    && (!s.EffectiveTo.HasValue || s.EffectiveTo.Value >= date));

            if (schedule == null)
            {
                return Json(new { available = false, message = "Doctor does not work on this day" });
            }

            // Check if time is within working hours
            if (time < schedule.StartTime || time >= schedule.EndTime)
            {
                return Json(new { available = false, message = "Time is outside working hours" });
            }

            // Check if time is during break
            if (schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue)
            {
                if (time >= schedule.BreakStartTime.Value && time < schedule.BreakEndTime.Value)
                {
                    return Json(new { available = false, message = "Time is during break" });
                }
            }

            // Check for holidays
            var holiday = await _context.DoctorHolidays
                .FirstOrDefaultAsync(h => h.DoctorId == doctorId
                    && h.TenantId == currentUser.TenantId
                    && h.IsApproved
                    && h.BlockNewAppointments
                    && date.Date >= h.StartDate.Date
                    && date.Date <= h.EndDate.Date);

            if (holiday != null)
            {
                if (holiday.IsFullDay)
                {
                    return Json(new { available = false, message = $"Doctor is on {holiday.Title}" });
                }
                else if (holiday.StartTime.HasValue && holiday.EndTime.HasValue)
                {
                    if (time >= holiday.StartTime.Value && time < holiday.EndTime.Value)
                    {
                        return Json(new { available = false, message = $"Doctor is on {holiday.Title}" });
                    }
                }
            }

            return Json(new { available = true, message = "Doctor is available" });
        }
    }
}


