using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Extensions;
using System.Linq.Expressions;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    // [Authorize(Roles = $"{SD.SuperAdminRole},{SD.AdminArea}")]
    public class AppointmentController : Controller
    {
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IRepository<Patient> _patientRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<AppointmentController> _localizer;
        private readonly ITenantService _tenantService;

        public AppointmentController(
            IRepository<Appointment> appointmentRepository,
            IRepository<Doctor> doctorRepository,
            IRepository<Patient> patientRepository,
            IRepository<Department> departmentRepository,
            ILocalizationService localizationService,
            IStringLocalizer<AppointmentController> localizer,
            ITenantService tenantService)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _departmentRepository = departmentRepository;
            _localizationService = localizationService;
            _localizer = localizer;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var appointments = await _appointmentRepository.GetAsync(
                    includes: new Expression<Func<Appointment, object>>[]
                    {
                        a => a.Doctor!,
                        a => a.Doctor!.ApplicationUser!,
                        a => a.Doctor!.Department!,
                        a => a.Patient!,
                        a => a.Patient!.ApplicationUser!,
                        a => a.Department!
                    },
                    sort: "DESC"
                );

                var viewModels = appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .Select(a => a.ToListViewModel())
                    .ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error loading appointments: {ex.Message}";
                return View(new List<AppointmentListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateAppointmentViewModel
            {
                AppointmentDate = DateTime.Today,
                AppointmentTime = new TimeSpan(9, 0, 0), // Default 9:00 AM
                DurationMinutes = 30
            };

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["error-notification"] = $"Validation failed: {string.Join(" | ", errors)}";
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                // Get tenant ID
                var currentTenant = _tenantService.GetFirstTenant();
                var tenantId = currentTenant?.TId ?? throw new InvalidOperationException("Tenant not found");

                // Check if appointment number already exists
                var existingAppointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.AppointmentNumber == viewModel.AppointmentNumber
                );

                if (existingAppointment != null)
                {
                    ModelState.AddModelError("AppointmentNumber", "Appointment number already exists");
                    TempData["error-notification"] = "Appointment number already exists";
                    await PopulateDropdownsAsync(viewModel);
                    return View(viewModel);
                }

                // Check for doctor availability (no overlapping appointments)
                var appointmentDateTime = viewModel.AppointmentDate.Date + viewModel.AppointmentTime;
                var endDateTime = appointmentDateTime.AddMinutes(viewModel.DurationMinutes);

                var overlappingAppointments = await _appointmentRepository.GetAsync(
                    expression: a => a.DoctorId == viewModel.DoctorId &&
                                   a.AppointmentDate == viewModel.AppointmentDate &&
                                   a.Status != "Cancelled" &&
                                   a.Status != "NoShow"
                );

                foreach (var existing in overlappingAppointments)
                {
                    var existingStart = existing.AppointmentDate.Date + existing.AppointmentTime;
                    var existingEnd = existingStart.AddMinutes(existing.DurationMinutes);

                    if ((appointmentDateTime >= existingStart && appointmentDateTime < existingEnd) ||
                        (endDateTime > existingStart && endDateTime <= existingEnd) ||
                        (appointmentDateTime <= existingStart && endDateTime >= existingEnd))
                    {
                        TempData["error-notification"] = "Doctor is not available at this time. Please choose a different time slot.";
                        await PopulateDropdownsAsync(viewModel);
                        return View(viewModel);
                    }
                }

                // Create appointment
                var createdBy = User.Identity?.Name ?? "System";
                var appointment = viewModel.ToEntity(tenantId, createdBy);

                await _appointmentRepository.CreateAsync(appointment);
                await _appointmentRepository.CommitAsync();

                var successMessage = _localizer["Appointment created successfully"];
                TempData["success-notification"] = successMessage.Value;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error creating appointment: {ex.Message}";
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.Id == id,
                    includes: new Expression<Func<Appointment, object>>[]
                    {
                        a => a.Doctor!,
                        a => a.Patient!,
                        a => a.Department!
                    }
                );

                if (appointment == null)
                {
                    TempData["error-notification"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = appointment.ToEditViewModel();
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error loading appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAppointmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["error-notification"] = $"Validation failed: {string.Join(" | ", errors)}";
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.Id == viewModel.Id
                );

                if (appointment == null)
                {
                    TempData["error-notification"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                // Check if appointment number is being changed and if it already exists
                if (appointment.AppointmentNumber != viewModel.AppointmentNumber)
                {
                    var existingAppointment = await _appointmentRepository.GetOneAsync(
                        expression: a => a.AppointmentNumber == viewModel.AppointmentNumber && a.Id != viewModel.Id
                    );

                    if (existingAppointment != null)
                    {
                        ModelState.AddModelError("AppointmentNumber", "Appointment number already exists");
                        TempData["error-notification"] = "Appointment number already exists";
                        await PopulateDropdownsAsync(viewModel);
                        return View(viewModel);
                    }
                }

                // Update appointment
                var modifiedBy = User.Identity?.Name ?? "System";
                appointment.UpdateFromViewModel(viewModel, modifiedBy);

                _appointmentRepository.Update(appointment);
                await _appointmentRepository.CommitAsync();

                var successMessage = _localizer["Appointment updated successfully"];
                TempData["success-notification"] = successMessage.Value;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error updating appointment: {ex.Message}";
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.Id == id
                );

                if (appointment == null)
                {
                    TempData["error-notification"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                _appointmentRepository.Delete(appointment);
                await _appointmentRepository.CommitAsync();

                var successMessage = _localizer["Appointment deleted successfully"];
                TempData["success-notification"] = successMessage.Value;
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error deleting appointment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.Id == id,
                    includes: new Expression<Func<Appointment, object>>[]
                    {
                        a => a.Doctor!,
                        a => a.Doctor!.ApplicationUser!,
                        a => a.Doctor!.Department!,
                        a => a.Patient!,
                        a => a.Patient!.ApplicationUser!,
                        a => a.Department!
                    }
                );

                if (appointment == null)
                {
                    TempData["error-notification"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error loading appointment details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string cancellationReason)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.Id == id
                );

                if (appointment == null)
                {
                    TempData["error-notification"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                appointment.Status = "Cancelled";
                appointment.IsCancelled = true;
                appointment.CancelledAt = DateTime.UtcNow;
                appointment.CancellationReason = cancellationReason;
                appointment.CancelledBy = User.Identity?.Name ?? "System";
                appointment.LastModified = DateTime.UtcNow;
                appointment.ModifiedBy = User.Identity?.Name ?? "System";

                _appointmentRepository.Update(appointment);
                await _appointmentRepository.CommitAsync();

                TempData["success-notification"] = "Appointment cancelled successfully";
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error cancelling appointment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    expression: a => a.Id == id
                );

                if (appointment == null)
                {
                    TempData["error-notification"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                appointment.IsConfirmed = true;
                appointment.ConfirmedAt = DateTime.UtcNow;
                appointment.ConfirmedBy = User.Identity?.Name ?? "System";
                appointment.Status = "Confirmed";
                appointment.LastModified = DateTime.UtcNow;
                appointment.ModifiedBy = User.Identity?.Name ?? "System";

                _appointmentRepository.Update(appointment);
                await _appointmentRepository.CommitAsync();

                TempData["success-notification"] = "Appointment confirmed successfully";
            }
            catch (Exception ex)
            {
                TempData["error-notification"] = $"Error confirming appointment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task PopulateDropdownsAsync(CreateAppointmentViewModel viewModel)
        {
            // Get doctors with their user information
            var doctors = await _doctorRepository.GetAsync(
                includes: new Expression<Func<Doctor, object>>[]
                {
                    d => d.ApplicationUser!,
                    d => d.Department!
                }
            );

            viewModel.Doctors = doctors.Select(d => new SelectListItem
            {
                Value = d.DoctorId,
                Text = $"{d.ApplicationUser?.Name} - {d.Department?.GetLocalizedName() ?? "N/A"}"
            }).ToList();

            // Get patients with their user information
            var patients = await _patientRepository.GetAsync(
                includes: new Expression<Func<Patient, object>>[]
                {
                    p => p.ApplicationUser!
                }
            );

            viewModel.Patients = patients.Select(p => new SelectListItem
            {
                Value = p.PatientId,
                Text = $"{p.ApplicationUser?.Name} - {p.PatientId}"
            }).ToList();

            // Get departments
            var departments = await _departmentRepository.GetAsync(
                includes: new Expression<Func<Department, object>>[]
                {
                    d => d.Translations!
                }
            );

            viewModel.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.GetLocalizedName()
            }).ToList();

            // Appointment types
            viewModel.AppointmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Consultation", Text = "Consultation" },
                new SelectListItem { Value = "Follow-up", Text = "Follow-up" },
                new SelectListItem { Value = "Emergency", Text = "Emergency" },
                new SelectListItem { Value = "Surgery", Text = "Surgery" },
                new SelectListItem { Value = "Checkup", Text = "Checkup" },
                new SelectListItem { Value = "Vaccination", Text = "Vaccination" },
                new SelectListItem { Value = "Lab Test", Text = "Lab Test" },
                new SelectListItem { Value = "Imaging", Text = "Imaging" },
                new SelectListItem { Value = "Therapy", Text = "Therapy" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            // Priorities
            viewModel.Priorities = new List<SelectListItem>
            {
                new SelectListItem { Value = "Normal", Text = "Normal", Selected = true },
                new SelectListItem { Value = "Urgent", Text = "Urgent" },
                new SelectListItem { Value = "Emergency", Text = "Emergency" }
            };
        }

        private async Task PopulateDropdownsAsync(EditAppointmentViewModel viewModel)
        {
            // Get doctors with their user information
            var doctors = await _doctorRepository.GetAsync(
                includes: new Expression<Func<Doctor, object>>[]
                {
                    d => d.ApplicationUser!,
                    d => d.Department!
                }
            );

            viewModel.Doctors = doctors.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.ApplicationUser?.Name} - {d.Department?.GetLocalizedName() ?? "N/A"}",
                Selected = d.Id == viewModel.DoctorId
            }).ToList();

            // Get patients with their user information
            var patients = await _patientRepository.GetAsync(
                includes: new Expression<Func<Patient, object>>[]
                {
                    p => p.ApplicationUser!
                }
            );

            viewModel.Patients = patients.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.ApplicationUser?.Name} - {p.PatientId}",
                Selected = p.Id == viewModel.PatientId
            }).ToList();

            // Get departments
            var departments = await _departmentRepository.GetAsync(
                includes: new Expression<Func<Department, object>>[]
                {
                    d => d.Translations!
                }
            );

            viewModel.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.GetLocalizedName(),
                Selected = d.Id == viewModel.DepartmentId
            }).ToList();

            // Appointment types
            viewModel.AppointmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Consultation", Text = "Consultation" },
                new SelectListItem { Value = "Follow-up", Text = "Follow-up" },
                new SelectListItem { Value = "Emergency", Text = "Emergency" },
                new SelectListItem { Value = "Surgery", Text = "Surgery" },
                new SelectListItem { Value = "Checkup", Text = "Checkup" },
                new SelectListItem { Value = "Vaccination", Text = "Vaccination" },
                new SelectListItem { Value = "Lab Test", Text = "Lab Test" },
                new SelectListItem { Value = "Imaging", Text = "Imaging" },
                new SelectListItem { Value = "Therapy", Text = "Therapy" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            // Priorities
            viewModel.Priorities = new List<SelectListItem>
            {
                new SelectListItem { Value = "Normal", Text = "Normal" },
                new SelectListItem { Value = "Urgent", Text = "Urgent" },
                new SelectListItem { Value = "Emergency", Text = "Emergency" }
            };

            // Statuses
            viewModel.Statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Scheduled", Text = "Scheduled" },
                new SelectListItem { Value = "Confirmed", Text = "Confirmed" },
                new SelectListItem { Value = "InProgress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "NoShow", Text = "No Show" },
                new SelectListItem { Value = "Rescheduled", Text = "Rescheduled" }
            };

            // Payment methods
            viewModel.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "Cash", Text = "Cash" },
                new SelectListItem { Value = "Card", Text = "Card" },
                new SelectListItem { Value = "Insurance", Text = "Insurance" },
                new SelectListItem { Value = "Online", Text = "Online" },
                new SelectListItem { Value = "Bank Transfer", Text = "Bank Transfer" }
            };
        }

        #endregion
    }
}
