using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Extensions;
using System.Linq.Expressions;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    // [Authorize(Roles = $"{SD.SuperAdminRole},{SD.AdminArea}")]
    public class DoctorController : Controller
    {
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<DoctorController> _localizer;
        private readonly ITenantService _tenantService;

        public DoctorController(
            IRepository<Doctor> doctorRepository,
            IRepository<Department> departmentRepository,
            UserManager<ApplicationUser> userManager,
            ILocalizationService localizationService,
            IStringLocalizer<DoctorController> localizer,
            ITenantService tenantService)
        {
            _doctorRepository = doctorRepository;
            _departmentRepository = departmentRepository;
            _userManager = userManager;
            _localizationService = localizationService;
            _localizer = localizer;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var doctors = await _doctorRepository.GetAsync(
                    includes: new Expression<Func<Doctor, object>>[] 
                    { 
                        d => d.ApplicationUser!,
                        d => d.Department!
                    },
                    sort: "DESC"
                );

                var doctorViewModels = doctors
                    .Select(d => d.ToListViewModel(_localizationService))
                    .ToList();

                return View(doctorViewModels);
            }
            catch (Exception)
            {
                // Database table doesn't exist yet
                TempData["error-notification"] = "Doctor functionality is under development. Database table not yet created.";
                return View(new List<DoctorListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateDoctorViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
                
                // Log validation errors for debugging
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join("; ", x.Value!.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                
                TempData["error-notification"] = $"Validation failed: {string.Join(" | ", errors)}";
                return View(viewModel);
            }

            try
            {
                // Check if DoctorId already exists
                var existingDoctor = await _doctorRepository.GetOneAsync(
                    expression: d => d.DoctorId == viewModel.DoctorId
                );

                if (existingDoctor != null)
                {
                    ModelState.AddModelError("DoctorId", "Doctor ID already exists");
                    await PopulateDropdownsAsync(viewModel);
                    return View(viewModel);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(viewModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    await PopulateDropdownsAsync(viewModel);
                    return View(viewModel);
                }

                var currentTenant = _tenantService.GetFirstTenant();
                var tenantId = currentTenant?.TId ?? throw new InvalidOperationException("Tenant not found");
                
                // Create ApplicationUser first
                var applicationUser = new ApplicationUser
                {
                    UserName = viewModel.Email,
                    Email = viewModel.Email,
                    PhoneNumber = viewModel.PhoneNumber,
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    Rate = viewModel.Rate,
                    TenantId = tenantId,
                    EmailConfirmed = true // Auto-confirm for admin created accounts
                };

                var userResult = await _userManager.CreateAsync(applicationUser, viewModel.Password);
                
                if (!userResult.Succeeded)
                {
                    foreach (var error in userResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await PopulateDropdownsAsync(viewModel);
                    return View(viewModel);
                }

                // Create Doctor with the new user ID
                var doctor = viewModel.ToEntity(tenantId, applicationUser.Id);
                
                await _doctorRepository.CreateAsync(doctor);
                await _doctorRepository.CommitAsync();

                var successMessage = _localizer["addDoctorSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var errorMessage = _localizer["invalidDoctorData"];
                TempData["error-notification"] = $"{errorMessage.Value} - Debug: {ex.Message}";
                
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _doctorRepository.GetOneAsync(
                expression: d => d.Id == id,
                includes: new Expression<Func<Doctor, object>>[] 
                { 
                    d => d.ApplicationUser!,
                    d => d.Department!
                }
            );

            if (doctor is null)
            {
                var notFoundMessage = _localizer["doctorNotFound"];
                TempData["error-notification"] = notFoundMessage.Value;
                return RedirectToAction(SD.NotFoundPage, SD.HomeController);
            }

            var viewModel = doctor.ToEditViewModel();
            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDoctorViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var existingDoctor = await _doctorRepository.GetOneAsync(
                    expression: d => d.Id == viewModel.Id
                );

                if (existingDoctor is null)
                {
                    var notFoundMessage = _localizer["doctorNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(nameof(Index));
                }

                // Check if DoctorId already exists for another doctor
                var doctorWithSameId = await _doctorRepository.GetOneAsync(
                    expression: d => d.DoctorId == viewModel.DoctorId && d.Id != viewModel.Id
                );

                if (doctorWithSameId != null)
                {
                    ModelState.AddModelError("DoctorId", "Doctor ID already exists");
                    await PopulateDropdownsAsync(viewModel);
                    return View(viewModel);
                }

                existingDoctor.UpdateFromViewModel(viewModel);
                
                _doctorRepository.Update(existingDoctor);
                await _doctorRepository.CommitAsync();

                var successMessage = _localizer["updateDoctorSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidDoctorData"];
                TempData["error-notification"] = errorMessage.Value;
                
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var doctor = await _doctorRepository.GetOneAsync(
                    expression: d => d.Id == id
                );

                if (doctor is null)
                {
                    var notFoundMessage = _localizer["doctorNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(SD.NotFoundPage, SD.HomeController);
                }

                _doctorRepository.Delete(doctor);
                await _doctorRepository.CommitAsync();

                var successMessage = _localizer["deleteDoctorSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidDoctorData"];
                TempData["error-notification"] = errorMessage.Value;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task PopulateDropdownsAsync(CreateDoctorViewModel viewModel)
        {
            // Get departments with translations
            var departments = await _departmentRepository.GetAsync(
                includes: new Expression<Func<Department, object>>[] { d => d.Translations }
            );
            viewModel.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.GetLocalizedName(_localizationService?.GetCurrentCulture() ?? "en")
            }).ToList();
        }

        private async Task PopulateDropdownsAsync(EditDoctorViewModel viewModel)
        {
            // Get application users for editing (still needed for editing existing doctors)
            var users = await _userManager.Users.ToListAsync();
            viewModel.ApplicationUsers = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.UserName} ({u.Email})"
            }).ToList();

            // Get departments with translations
            var departments = await _departmentRepository.GetAsync(
                includes: new Expression<Func<Department, object>>[] { d => d.Translations }
            );
            viewModel.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.GetLocalizedName(_localizationService?.GetCurrentCulture() ?? "en")
            }).ToList();
        }
    }
}