using Clinics_Websites_Shops.Areas.Admin.ViewModels;
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
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrescriptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Prescription
        [Permission("Prescriptions.View")]
        public async Task<IActionResult> Index()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(p => p.Appointment)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // GET: Admin/Prescription/Details/5
        [Permission("Prescriptions.View")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(p => p.Appointment)
                .Include(p => p.Report)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // GET: Admin/Prescription/Create
        [Permission("Prescriptions.Create")]
        public async Task<IActionResult> Create(int? patientId, int? appointmentId)
        {
            await PopulateDropdowns(patientId, appointmentId);
            
            var model = new PrescriptionViewModel
            {
                PrescriptionNumber = GeneratePrescriptionNumber(),
                PrescriptionDate = DateTime.Now,
                PatientId = patientId ?? 0,
                AppointmentId = appointmentId
            };

            return View(model);
        }

        // POST: Admin/Prescription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Prescriptions.Create")]
        public async Task<IActionResult> Create(PrescriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                var prescription = new Prescription
                {
                    PrescriptionNumber = model.PrescriptionNumber,
                    PrescriptionDate = model.PrescriptionDate,
                    MedicationName = model.MedicationName,
                    Dosage = model.Dosage,
                    Frequency = model.Frequency,
                    Duration = model.Duration,
                    Quantity = model.Quantity,
                    Route = model.Route,
                    Instructions = model.Instructions,
                    Notes = model.Notes,
                    IsActive = model.IsActive,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    AppointmentId = model.AppointmentId,
                    ReportId = model.ReportId,
                    TenantId = currentUser.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.Id
                };

                _context.Add(prescription);
                await _context.SaveChangesAsync();
                
                TempData["success"] = "Prescription created successfully";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model.PatientId, model.AppointmentId);
            return View(model);
        }

        // GET: Admin/Prescription/Edit/5
        [Permission("Prescriptions.Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            var model = new PrescriptionViewModel
            {
                Id = prescription.Id,
                PrescriptionNumber = prescription.PrescriptionNumber,
                PrescriptionDate = prescription.PrescriptionDate,
                MedicationName = prescription.MedicationName,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                Duration = prescription.Duration,
                Quantity = prescription.Quantity,
                Route = prescription.Route,
                Instructions = prescription.Instructions,
                Notes = prescription.Notes,
                IsActive = prescription.IsActive,
                StartDate = prescription.StartDate,
                EndDate = prescription.EndDate,
                PatientId = prescription.PatientId,
                DoctorId = prescription.DoctorId,
                AppointmentId = prescription.AppointmentId,
                ReportId = prescription.ReportId
            };

            await PopulateDropdowns(model.PatientId, model.AppointmentId);
            return View(model);
        }

        // POST: Admin/Prescription/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Prescriptions.Edit")]
        public async Task<IActionResult> Edit(int id, PrescriptionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = await _context.Prescriptions.FindAsync(id);
                    if (prescription == null)
                    {
                        return NotFound();
                    }

                    var currentUser = await _userManager.GetUserAsync(User);

                    prescription.PrescriptionNumber = model.PrescriptionNumber;
                    prescription.PrescriptionDate = model.PrescriptionDate;
                    prescription.MedicationName = model.MedicationName;
                    prescription.Dosage = model.Dosage;
                    prescription.Frequency = model.Frequency;
                    prescription.Duration = model.Duration;
                    prescription.Quantity = model.Quantity;
                    prescription.Route = model.Route;
                    prescription.Instructions = model.Instructions;
                    prescription.Notes = model.Notes;
                    prescription.IsActive = model.IsActive;
                    prescription.StartDate = model.StartDate;
                    prescription.EndDate = model.EndDate;
                    prescription.PatientId = model.PatientId;
                    prescription.DoctorId = model.DoctorId;
                    prescription.AppointmentId = model.AppointmentId;
                    prescription.ReportId = model.ReportId;
                    prescription.LastModified = DateTime.UtcNow;
                    prescription.ModifiedBy = currentUser.Id;

                    _context.Update(prescription);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Prescription updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PopulateDropdowns(model.PatientId, model.AppointmentId);
            return View(model);
        }

        // GET: Admin/Prescription/Delete/5
        [Permission("Prescriptions.Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // POST: Admin/Prescription/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission("Prescriptions.Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
                TempData["success"] = "Prescription deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.Id == id);
        }

        private async Task PopulateDropdowns(int? selectedPatientId = null, int? selectedAppointmentId = null)
        {
            ViewBag.Patients = new SelectList(
                await _context.Patients
                    .Include(p => p.ApplicationUser)
                    .Select(p => new { p.Id, Name = p.ApplicationUser.Name })
                    .ToListAsync(),
                "Id", "Name", selectedPatientId);

            ViewBag.Doctors = new SelectList(
                await _context.Doctors
                    .Include(d => d.ApplicationUser)
                    .Select(d => new { d.Id, Name = d.ApplicationUser.Name })
                    .ToListAsync(),
                "Id", "Name");

            ViewBag.Appointments = new SelectList(
                await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.ApplicationUser)
                    .Select(a => new { a.Id, Display = a.AppointmentNumber + " - " + a.Patient.ApplicationUser.Name })
                    .ToListAsync(),
                "Id", "Display", selectedAppointmentId);

            ViewBag.Reports = new SelectList(
                await _context.Reports
                    .Include(r => r.Patient)
                        .ThenInclude(p => p.ApplicationUser)
                    .Select(r => new { r.Id, Display = "Report #" + r.Id + " - " + r.Patient.ApplicationUser.Name })
                    .ToListAsync(),
                "Id", "Display");
        }

        private string GeneratePrescriptionNumber()
        {
            return "RX" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}

