using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Services.IServices;
using Clinics_Websites_Shops.Extensions;
using System.Linq.Expressions;

namespace Clinics_Websites_Shops.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    // [Authorize(Roles = $"{SD.SuperAdminRole},{SD.AdminArea}")]
    public class DepartmentController : Controller
    {
        private readonly IRepository<Department> _departmentRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<DepartmentController> _localizer;
        private readonly ITenantService _tenantService;

        public DepartmentController(
            IRepository<Department> departmentRepository,
            ILocalizationService localizationService,
            IStringLocalizer<DepartmentController> localizer,
            ITenantService tenantService)
        {
            _departmentRepository = departmentRepository;
            _localizationService = localizationService;
            _localizer = localizer;
            _tenantService = tenantService;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _departmentRepository.GetAsync(
                includes: new Expression<Func<Department, object>>[] { d => d.Translations },
                sort: "DESC"
            );

            var departmentViewModels = departments
                .Select(d => d.ToListViewModel(_localizationService))
                .ToList();

            return View(departmentViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var supportedLanguages = _localizationService.GetSupportedLanguages();
            var viewModel = new CreateDepartmentViewModel();
            
            // Initialize translations for all supported languages
            foreach (var language in supportedLanguages)
            {
                viewModel.Translations.Add(new DepartmentTranslationViewModel
                {
                    LanguageCode = language.Key,
                    LanguageName = language.Value,
                    Name = string.Empty,
                    Description = string.Empty
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate language names in case of validation errors
                var supportedLanguages = _localizationService.GetSupportedLanguages();
                foreach (var translation in viewModel.Translations)
                {
                    if (supportedLanguages.ContainsKey(translation.LanguageCode))
                    {
                        translation.LanguageName = supportedLanguages[translation.LanguageCode];
                    }
                }
                return View(viewModel);
            }

            try
            {
                var currentTenant = _tenantService.GetCurrentTenant(HttpContext);
                var tenantId = currentTenant?.TId ?? throw new InvalidOperationException("Tenant not found");
                var department = viewModel.ToEntity(tenantId);
                
                await _departmentRepository.CreateAsync(department);
                await _departmentRepository.CommitAsync();

                var successMessage = _localizer["addDepartmentSuccess"];
                TempData["success-notification"] = successMessage.Value;
                Response.Cookies.Append("success-notification", successMessage.Value, new()
                {
                    Secure = true,
                    Expires = DateTime.Now.AddDays(1)
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidDepartmentData"];
                TempData["error-notification"] = errorMessage.Value;
                
                // Repopulate language names in case of error
                var supportedLanguages = _localizationService.GetSupportedLanguages();
                foreach (var translation in viewModel.Translations)
                {
                    if (supportedLanguages.ContainsKey(translation.LanguageCode))
                    {
                        translation.LanguageName = supportedLanguages[translation.LanguageCode];
                    }
                }
                
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentRepository.GetOneAsync(
                expression: d => d.Id == id,
                includes: new Expression<Func<Department, object>>[] { d => d.Translations }
            );

            if (department is null)
            {
                var notFoundMessage = _localizer["departmentNotFound"];
                TempData["error-notification"] = notFoundMessage.Value;
                return RedirectToAction(SD.NotFoundPage, SD.HomeController);
            }

            var supportedLanguages = _localizationService.GetSupportedLanguages();
            var viewModel = department.ToEditViewModel(supportedLanguages);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDepartmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate language names in case of validation errors
                var supportedLanguages = _localizationService.GetSupportedLanguages();
                foreach (var translation in viewModel.Translations)
                {
                    if (supportedLanguages.ContainsKey(translation.LanguageCode))
                    {
                        translation.LanguageName = supportedLanguages[translation.LanguageCode];
                    }
                }
                return View(viewModel);
            }

            try
            {
                var existingDepartment = await _departmentRepository.GetOneAsync(
                    expression: d => d.Id == viewModel.Id,
                    includes: new Expression<Func<Department, object>>[] { d => d.Translations }
                );

                if (existingDepartment is null)
                {
                    var notFoundMessage = _localizer["departmentNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(nameof(Index));
                }

                existingDepartment.UpdateFromViewModel(viewModel);
                
                _departmentRepository.Update(existingDepartment);
                await _departmentRepository.CommitAsync();

                var successMessage = _localizer["updateDepartmentSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidDepartmentData"];
                TempData["error-notification"] = errorMessage.Value;
                
                // Repopulate language names in case of error
                var supportedLanguages = _localizationService.GetSupportedLanguages();
                foreach (var translation in viewModel.Translations)
                {
                    if (supportedLanguages.ContainsKey(translation.LanguageCode))
                    {
                        translation.LanguageName = supportedLanguages[translation.LanguageCode];
                    }
                }
                
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var department = await _departmentRepository.GetOneAsync(
                    expression: d => d.Id == id,
                    includes: new Expression<Func<Department, object>>[] { d => d.Translations }
                );

                if (department is null)
                {
                    var notFoundMessage = _localizer["departmentNotFound"];
                    TempData["error-notification"] = notFoundMessage.Value;
                    return RedirectToAction(SD.NotFoundPage, SD.HomeController);
                }

                _departmentRepository.Delete(department);
                await _departmentRepository.CommitAsync();

                var successMessage = _localizer["deleteDepartmentSuccess"];
                TempData["success-notification"] = successMessage.Value;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                var errorMessage = _localizer["invalidDepartmentData"];
                TempData["error-notification"] = errorMessage.Value;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
