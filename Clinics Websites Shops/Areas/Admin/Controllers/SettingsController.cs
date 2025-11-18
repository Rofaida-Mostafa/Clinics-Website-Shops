using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(
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

            var settings = await _context.ClinicSettings
                .FirstOrDefaultAsync(s => s.TenantId == currentUser.TenantId);

            if (settings == null)
            {
                // Create default settings for this tenant
                settings = new ClinicSettings
                {
                    TenantId = currentUser.TenantId,
                    ClinicName = "My Clinic",
                    DefaultAppointmentDuration = 30,
                    AppointmentSlotInterval = 15,
                    AllowOnlineBooking = true,
                    RequireAppointmentConfirmation = true,
                    ReminderHoursBefore = 24,
                    SendSmsReminders = false,
                    SendEmailReminders = true,
                    Currency = "USD",
                    CurrencySymbol = "$",
                    TaxRate = 0,
                    RequirePaymentAtBooking = false,
                    EnableEmailNotifications = true,
                    EnableSmsNotifications = false,
                    SmtpUseSsl = true,
                    DefaultLanguage = "en",
                    DefaultCulture = "en-US",
                    SupportedLanguages = "en,ar",
                    SessionTimeoutMinutes = 60,
                    RequireTwoFactorAuth = false,
                    PasswordExpiryDays = 90,
                    MaxLoginAttempts = 5,
                    EnablePatientPortal = true,
                    EnableOnlinePayments = false,
                    EnableTelehealth = false,
                    EnablePrescriptionManagement = true,
                    EnableLabResults = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ClinicSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        [HttpPost]
        [Permission("Settings.Manage")]
        public async Task<IActionResult> Update(ClinicSettings model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var settings = await _context.ClinicSettings
                .FirstOrDefaultAsync(s => s.TenantId == currentUser.TenantId);

            if (settings == null)
            {
                TempData["error"] = "Settings not found";
                return RedirectToAction(nameof(Index));
            }

            // Update settings
            settings.ClinicName = model.ClinicName;
            settings.ClinicDescription = model.ClinicDescription;
            settings.Address = model.Address;
            settings.City = model.City;
            settings.State = model.State;
            settings.PostalCode = model.PostalCode;
            settings.Country = model.Country;
            settings.Phone = model.Phone;
            settings.Fax = model.Fax;
            settings.Email = model.Email;
            settings.Website = model.Website;
            settings.BusinessHours = model.BusinessHours;
            settings.TimeZone = model.TimeZone;
            settings.DefaultAppointmentDuration = model.DefaultAppointmentDuration;
            settings.AppointmentSlotInterval = model.AppointmentSlotInterval;
            settings.AllowOnlineBooking = model.AllowOnlineBooking;
            settings.RequireAppointmentConfirmation = model.RequireAppointmentConfirmation;
            settings.ReminderHoursBefore = model.ReminderHoursBefore;
            settings.SendSmsReminders = model.SendSmsReminders;
            settings.SendEmailReminders = model.SendEmailReminders;
            settings.Currency = model.Currency;
            settings.CurrencySymbol = model.CurrencySymbol;
            settings.DefaultConsultationFee = model.DefaultConsultationFee;
            settings.TaxRate = model.TaxRate;
            settings.RequirePaymentAtBooking = model.RequirePaymentAtBooking;
            settings.EnableEmailNotifications = model.EnableEmailNotifications;
            settings.EnableSmsNotifications = model.EnableSmsNotifications;
            settings.DefaultLanguage = model.DefaultLanguage;
            settings.DefaultCulture = model.DefaultCulture;
            settings.SupportedLanguages = model.SupportedLanguages;
            settings.SessionTimeoutMinutes = model.SessionTimeoutMinutes;
            settings.RequireTwoFactorAuth = model.RequireTwoFactorAuth;
            settings.PasswordExpiryDays = model.PasswordExpiryDays;
            settings.MaxLoginAttempts = model.MaxLoginAttempts;
            settings.EnablePatientPortal = model.EnablePatientPortal;
            settings.EnableOnlinePayments = model.EnableOnlinePayments;
            settings.EnableTelehealth = model.EnableTelehealth;
            settings.EnablePrescriptionManagement = model.EnablePrescriptionManagement;
            settings.EnableLabResults = model.EnableLabResults;
            settings.LastModified = DateTime.UtcNow;
            settings.ModifiedBy = currentUser.Id;

            await _context.SaveChangesAsync();

            TempData["success"] = "Settings updated successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}

