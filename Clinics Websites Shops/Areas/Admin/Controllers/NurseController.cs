using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Attributes;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Extensions;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Linq.Expressions;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area("Admin")]
  [Authorize]
    public class NurseController : Controller
    {
        private readonly IRepository<Nurse> _nurseRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<NurseController> _localizer;

        public NurseController(
            IRepository<Nurse> nurseRepository,
            UserManager<ApplicationUser> userManager,
            ITenantService tenantService,
            ILocalizationService localizationService,
            IStringLocalizer<NurseController> localizer)
        {
            _nurseRepository = nurseRepository;
            _userManager = userManager;
            _tenantService = tenantService;
            _localizationService = localizationService;
            _localizer = localizer;
        }

      [Permission("Nurses.View")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var nurses = await _nurseRepository.GetAsync(
                    includes: new Expression<Func<Nurse, object>>[]
                    {
                        n => n.ApplicationUser!
                    },
                    sort: "DESC"
                );

                var nurseViewModels = nurses
                    .Select(n => n.ToListViewModel(_localizationService))
                    .ToList();

                return View(nurseViewModels);
            }
            catch (Exception)
            {
                // Database table doesn't exist yet
                TempData["error-notification"] = "Nurse functionality is under development. Database table not yet created.";
                return View(new List<NurseListViewModel>());
            }
        }

        [HttpGet]
        [Permission("Nurses.Create")]
        public IActionResult Create()
        {
            var viewModel = new CreateNurseViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [Permission("Nurses.Create")]
        public async Task<IActionResult> Create(CreateNurseViewModel viewModel)
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
                // Check if NurseId already exists
                var existingNurse = await _nurseRepository.GetOneAsync(
                    expression: n => n.NurseId == viewModel.NurseId
                );

                if (existingNurse != null)
                {
                    ModelState.AddModelError("NurseId", "Nurse ID already exists");
                    return View(viewModel);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(viewModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
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
                    return View(viewModel);
                }

                // Create Nurse with the new user ID
                var nurse = viewModel.ToEntity(tenantId, applicationUser.Id);
                
                await _nurseRepository.CreateAsync(nurse);
                await _nurseRepository.CommitAsync();

                var successMessage = _localizer["addNurseSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var errorMessage = _localizer["invalidNurseData"];
                TempData["error-notification"] = $"{errorMessage.Value} - Debug: {ex.Message}";
                
                return View(viewModel);
            }
        }

        [HttpGet]
        [Permission("Nurses.Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var nurse = await _nurseRepository.GetOneAsync(
                expression: n => n.NurseId == id,
                includes: new Expression<Func<Nurse, object>>[]
                {
                    n => n.ApplicationUser!
                }
            );

            if (nurse is null)
            {
                var notFoundMessage = _localizer["nurseNotFound"];
                TempData["error-notification"] = notFoundMessage.Value;
                return RedirectToAction(SD.NotFoundPage, SD.HomeController);
            }

            var viewModel = nurse.ToEditViewModel();

            return View(viewModel);
        }

        [HttpPost]
        [Permission("Nurses.Edit")]
        public async Task<IActionResult> Edit(EditNurseViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var existingNurse = await _nurseRepository.GetOneAsync(
                    expression: n => n.NurseId == viewModel.NurseId
                );

                if (existingNurse is null)
                {
                    var notFoundMessage = _localizer["nurseNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(nameof(Index));
                }

                // NurseId cannot be changed in edit, so no need to check for duplicates

                existingNurse.UpdateFromViewModel(viewModel);
                
                _nurseRepository.Update(existingNurse);
                await _nurseRepository.CommitAsync();

                var successMessage = _localizer["updateNurseSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidNurseData"];
                TempData["error-notification"] = errorMessage.Value;
                
                return View(viewModel);
            }
        }

        [Permission("Nurses.View")]
        public async Task<IActionResult> Details(string id)
        {
            var nurse = await _nurseRepository.GetOneAsync(
                expression: n => n.NurseId == id,
                includes: new Expression<Func<Nurse, object>>[]
                {
                    n => n.ApplicationUser!
                }
            );

            if (nurse is null)
            {
                var notFoundMessage = _localizer["nurseNotFound"];
                TempData["error-notification"] = notFoundMessage.Value;
                return RedirectToAction(SD.NotFoundPage, SD.HomeController);
            }

            return View(nurse);
        }

        [HttpPost]
        [Permission("Nurses.Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var nurse = await _nurseRepository.GetOneAsync(
                    expression: n => n.NurseId == id
                );

                if (nurse is null)
                {
                    var notFoundMessage = _localizer["nurseNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(SD.NotFoundPage, SD.HomeController);
                }

                _nurseRepository.Delete(nurse);
                await _nurseRepository.CommitAsync();

                var successMessage = _localizer["deleteNurseSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidNurseData"];
                TempData["error-notification"] = errorMessage.Value;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

