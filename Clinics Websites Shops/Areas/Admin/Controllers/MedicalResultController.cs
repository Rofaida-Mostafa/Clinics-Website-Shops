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
    public class MedicalResultController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MedicalResultController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/MedicalResult
        [Permission("Results.View")]
        public async Task<IActionResult> Index()
        {
            var results = await _context.MedicalResults
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Appointment)
                .OrderByDescending(r => r.TestDate)
                .ToListAsync();

            return View(results);
        }

        // GET: Admin/MedicalResult/Details/5
        [Permission("Results.View")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.MedicalResults
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Appointment)
                .Include(r => r.Report)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // GET: Admin/MedicalResult/Create
        [Permission("Results.Create")]
        public async Task<IActionResult> Create(int? patientId, int? appointmentId)
        {
            await PopulateDropdowns(patientId, appointmentId);
            
            var model = new MedicalResultViewModel
            {
                ResultNumber = GenerateResultNumber(),
                TestDate = DateTime.Now,
                PatientId = patientId ?? 0,
                AppointmentId = appointmentId,
                Status = "Pending"
            };

            return View(model);
        }

        // POST: Admin/MedicalResult/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Results.Create")]
        public async Task<IActionResult> Create(MedicalResultViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                var result = new MedicalResult
                {
                    ResultNumber = model.ResultNumber,
                    TestName = model.TestName,
                    TestCategory = model.TestCategory,
                    TestCode = model.TestCode,
                    TestDate = model.TestDate,
                    ResultDate = model.ResultDate,
                    Status = model.Status,
                    ResultValue = model.ResultValue,
                    Findings = model.Findings,
                    Notes = model.Notes,
                    IsAbnormal = model.IsAbnormal,
                    IsCritical = model.IsCritical,
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    AppointmentId = model.AppointmentId,
                    ReportId = model.ReportId,
                    TenantId = currentUser.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.Id
                };

                // Handle file upload
                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    var uploadResult = await UploadFile(model.AttachmentFile);
                    result.AttachmentUrl = uploadResult.Url;
                    result.AttachmentFileName = uploadResult.FileName;
                    result.AttachmentFileType = uploadResult.FileType;
                }

                _context.Add(result);
                await _context.SaveChangesAsync();
                
                TempData["success"] = "Medical result created successfully";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model.PatientId, model.AppointmentId);
            return View(model);
        }

        // GET: Admin/MedicalResult/Edit/5
        [Permission("Results.Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.MedicalResults.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            var model = new MedicalResultViewModel
            {
                Id = result.Id,
                ResultNumber = result.ResultNumber,
                TestName = result.TestName,
                TestCategory = result.TestCategory,
                TestCode = result.TestCode,
                TestDate = result.TestDate,
                ResultDate = result.ResultDate,
                Status = result.Status,
                ResultValue = result.ResultValue,
                Findings = result.Findings,
                Notes = result.Notes,
                IsAbnormal = result.IsAbnormal,
                IsCritical = result.IsCritical,
                PatientId = result.PatientId,
                DoctorId = result.DoctorId,
                AppointmentId = result.AppointmentId,
                ReportId = result.ReportId,
                AttachmentUrl = result.AttachmentUrl,
                AttachmentFileName = result.AttachmentFileName
            };

            await PopulateDropdowns(model.PatientId, model.AppointmentId);
            return View(model);
        }

        // POST: Admin/MedicalResult/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Results.Edit")]
        public async Task<IActionResult> Edit(int id, MedicalResultViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _context.MedicalResults.FindAsync(id);
                    if (result == null)
                    {
                        return NotFound();
                    }

                    var currentUser = await _userManager.GetUserAsync(User);

                    result.ResultNumber = model.ResultNumber;
                    result.TestName = model.TestName;
                    result.TestCategory = model.TestCategory;
                    result.TestCode = model.TestCode;
                    result.TestDate = model.TestDate;
                    result.ResultDate = model.ResultDate;
                    result.Status = model.Status;
                    result.ResultValue = model.ResultValue;
                    result.Findings = model.Findings;
                    result.Notes = model.Notes;
                    result.IsAbnormal = model.IsAbnormal;
                    result.IsCritical = model.IsCritical;
                    result.PatientId = model.PatientId;
                    result.DoctorId = model.DoctorId;
                    result.AppointmentId = model.AppointmentId;
                    result.ReportId = model.ReportId;
                    result.LastModified = DateTime.UtcNow;
                    result.ModifiedBy = currentUser.Id;

                    // Handle file upload
                    if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(result.AttachmentUrl))
                        {
                            DeleteFile(result.AttachmentUrl);
                        }

                        var uploadResult = await UploadFile(model.AttachmentFile);
                        result.AttachmentUrl = uploadResult.Url;
                        result.AttachmentFileName = uploadResult.FileName;
                        result.AttachmentFileType = uploadResult.FileType;
                    }

                    _context.Update(result);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Medical result updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalResultExists(model.Id))
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

        // GET: Admin/MedicalResult/Delete/5
        [Permission("Results.Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.MedicalResults
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // POST: Admin/MedicalResult/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission("Results.Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _context.MedicalResults.FindAsync(id);
            if (result != null)
            {
                // Delete file if exists
                if (!string.IsNullOrEmpty(result.AttachmentUrl))
                {
                    DeleteFile(result.AttachmentUrl);
                }

                _context.MedicalResults.Remove(result);
                await _context.SaveChangesAsync();
                TempData["success"] = "Medical result deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MedicalResultExists(int id)
        {
            return _context.MedicalResults.Any(e => e.Id == id);
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

        private string GenerateResultNumber()
        {
            return "LAB" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private async Task<(string Url, string FileName, string FileType)> UploadFile(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "results");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return (
                Url: "/uploads/results/" + uniqueFileName,
                FileName: file.FileName,
                FileType: file.ContentType
            );
        }

        private void DeleteFile(string fileUrl)
        {
            if (!string.IsNullOrEmpty(fileUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}

