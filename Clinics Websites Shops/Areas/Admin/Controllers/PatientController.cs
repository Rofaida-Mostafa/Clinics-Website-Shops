using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Identity;
using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Extensions;
using System.Linq.Expressions;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    // [Authorize(Roles = $"{SD.SuperAdminRole},{SD.AdminArea}")]
    public class PatientController : Controller
    {
        private readonly IRepository<Patient> _patientRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<PatientController> _localizer;
        private readonly ITenantService _tenantService;

        public PatientController(
            IRepository<Patient> patientRepository,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<PatientController> localizer,
            ITenantService tenantService)
        {
            _patientRepository = patientRepository;
            _userManager = userManager;
            _localizer = localizer;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var patients = await _patientRepository.GetAsync(
                    includes: new Expression<Func<Patient, object>>[] 
                    { 
                        p => p.ApplicationUser!
                    },
                    sort: "DESC"
                );

                var patientViewModels = patients
                    .Select(p => p.ToListViewModel())
                    .ToList();

                return View(patientViewModels);
            }
            catch (Exception)
            {
                // Database table doesn't exist yet
                TempData["error-notification"] = "Patient functionality is under development. Database table not yet created.";
                return View(new List<PatientListViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CreatePatientViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePatientViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
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
                // Get tenant ID
                var currentTenant = _tenantService.GetFirstTenant();
                var tenantId = currentTenant?.TId ?? throw new InvalidOperationException("Tenant not found");

                // Check if PatientId already exists
                var existingPatient = await _patientRepository.GetOneAsync(
                    expression: p => p.PatientId == viewModel.PatientId
                );

                if (existingPatient != null)
                {
                    ModelState.AddModelError("PatientId", "Patient ID already exists");
                    return View(viewModel);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(viewModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(viewModel);
                }

                // Create ApplicationUser
                var applicationUser = new ApplicationUser
                {
                    UserName = viewModel.Email,
                    Email = viewModel.Email,
                    Name = viewModel.Name,
                    PhoneNumber = viewModel.PhoneNumber,
                    Description = viewModel.Description,
                    Rate = viewModel.Rate,
                    TenantId = tenantId
                };

                var userResult = await _userManager.CreateAsync(applicationUser, viewModel.Password);
                
                if (!userResult.Succeeded)
                {
                    foreach (var error in userResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(viewModel);
                }

                // Create Patient with the new user ID
                var patient = viewModel.ToEntity(tenantId, applicationUser.Id);
                
                await _patientRepository.CreateAsync(patient);
                await _patientRepository.CommitAsync();

                var successMessage = _localizer["addPatientSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var errorMessage = _localizer["invalidPatientData"];
                TempData["error-notification"] = $"{errorMessage.Value} - Debug: {ex.Message}";
                
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientRepository.GetOneAsync(
                expression: p => p.Id == id,
                includes: new Expression<Func<Patient, object>>[] 
                { 
                    p => p.ApplicationUser!
                }
            );

            if (patient is null)
            {
                var notFoundMessage = _localizer["patientNotFound"];
                TempData["error-notification"] = notFoundMessage.Value;
                return RedirectToAction(SD.NotFoundPage, SD.HomeController);
            }

            var viewModel = patient.ToEditViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditPatientViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var existingPatient = await _patientRepository.GetOneAsync(
                    expression: p => p.Id == viewModel.Id
                );

                if (existingPatient is null)
                {
                    var notFoundMessage = _localizer["patientNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(nameof(Index));
                }

                // Check if PatientId already exists for another patient
                var patientWithSameId = await _patientRepository.GetOneAsync(
                    expression: p => p.PatientId == viewModel.PatientId && p.Id != viewModel.Id
                );

                if (patientWithSameId != null)
                {
                    ModelState.AddModelError("PatientId", "Patient ID already exists");
                    return View(viewModel);
                }

                existingPatient.UpdateFromViewModel(viewModel);
                
                _patientRepository.Update(existingPatient);
                await _patientRepository.CommitAsync();

                var successMessage = _localizer["updatePatientSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidPatientData"];
                TempData["error-notification"] = errorMessage.Value;
                
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var patient = await _patientRepository.GetOneAsync(
                    expression: p => p.Id == id
                );

                if (patient is null)
                {
                    var notFoundMessage = _localizer["patientNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(SD.NotFoundPage, SD.HomeController);
                }

                _patientRepository.Delete(patient);
                await _patientRepository.CommitAsync();

                var successMessage = _localizer["deletePatientSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidPatientData"];
                TempData["error-notification"] = errorMessage.Value;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

