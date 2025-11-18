using System.Globalization;
using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Services.IServices;

namespace Clinics_Websites_Shops.Extensions
{
    /// <summary>
    /// Extension methods for Department entity to simplify localization
    /// </summary>
    public static class DepartmentExtensions
    {
        /// <summary>
        /// Gets the localized name for the department based on current culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <returns>Localized name or default name if translation not found</returns>
        public static string GetLocalizedName(this Department department)
        {
            if (department == null)
                return string.Empty;

            var currentCulture = CultureInfo.CurrentCulture.Name;
            return department.GetLocalizedName(currentCulture);
        }

        /// <summary>
        /// Gets the localized name for the department for a specific culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Culture code (e.g., "en-US", "ar-EG", "de-DE")</param>
        /// <returns>Localized name or default name if translation not found</returns>
        public static string GetLocalizedName(this Department department, string culture)
        {
            if (department == null)
                return string.Empty;

            if (string.IsNullOrEmpty(culture))
            {
                // Fallback to first available translation (preferably English)
                var fallback = department.Translations?
                    .FirstOrDefault(t => t.LanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    ?? department.Translations?.FirstOrDefault();
                return fallback?.Name ?? $"Department {department.Id}";
            }

            var translation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.Equals(culture, StringComparison.OrdinalIgnoreCase));

            if (translation != null)
                return translation.Name;

            // Fallback to first available translation (preferably English)
            var fallbackTranslation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                ?? department.Translations?.FirstOrDefault();

            return fallbackTranslation?.Name ?? $"Department {department.Id}";
        }

        /// <summary>
        /// Checks if a translation exists for the specified culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Culture code</param>
        /// <returns>True if translation exists, false otherwise</returns>
        public static bool HasTranslation(this Department department, string culture)
        {
            if (department?.Translations == null)
                return false;

            return department.Translations.Any(t => 
                t.LanguageCode.Equals(culture, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all available translations for the department
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <returns>Dictionary of culture codes and translated names</returns>
        public static Dictionary<string, string> GetAllTranslations(this Department department)
        {
            if (department?.Translations == null)
                return new Dictionary<string, string>();

            return department.Translations.ToDictionary(
                t => t.LanguageCode,
                t => t.Name
            );
        }

        /// <summary>
        /// Adds or updates a translation for the department
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Culture code</param>
        /// <param name="translatedName">Translated name</param>
        public static void SetTranslation(this Department department, string culture, string translatedName)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            if (string.IsNullOrEmpty(culture))
                throw new ArgumentException("Culture cannot be null or empty", nameof(culture));

            if (string.IsNullOrEmpty(translatedName))
                throw new ArgumentException("Translated name cannot be null or empty", nameof(translatedName));

            department.Translations ??= new List<DepartmentTranslation>();

            var existingTranslation = department.Translations
                .FirstOrDefault(t => t.LanguageCode.Equals(culture, StringComparison.OrdinalIgnoreCase));

            if (existingTranslation != null)
            {
                existingTranslation.Name = translatedName;
            }
            else
            {
                department.Translations.Add(new DepartmentTranslation
                {
                    DepartmentId = department.Id,
                    LanguageCode = culture,
                    Name = translatedName
                });
            }
        }

        /// <summary>
        /// Removes a translation for the specified culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Culture code</param>
        /// <returns>True if translation was removed, false if not found</returns>
        public static bool RemoveTranslation(this Department department, string culture)
        {
            if (department?.Translations == null)
                return false;

            var translation = department.Translations
                .FirstOrDefault(t => t.LanguageCode.Equals(culture, StringComparison.OrdinalIgnoreCase));

            if (translation != null)
            {
                department.Translations.Remove(translation);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the localized description for the department based on current culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <returns>Localized description or default description if translation not found</returns>
        public static string GetLocalizedDescription(this Department department)
        {
            if (department == null)
                return string.Empty;

            var currentCulture = CultureInfo.CurrentCulture.Name;
            return department.GetLocalizedDescription(currentCulture);
        }

        /// <summary>
        /// Gets the localized description for the department for a specific culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Culture code (e.g., "en-US", "ar-EG", "de-DE")</param>
        /// <returns>Localized description or default description if translation not found</returns>
        public static string GetLocalizedDescription(this Department department, string culture)
        {
            if (department == null)
                return string.Empty;

            if (string.IsNullOrEmpty(culture))
            {
                // Fallback to first available translation (preferably English)
                var fallback = department.Translations?
                    .FirstOrDefault(t => t.LanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    ?? department.Translations?.FirstOrDefault();
                return fallback?.Description ?? string.Empty;
            }

            var translation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.Equals(culture, StringComparison.OrdinalIgnoreCase));

            if (translation != null)
                return translation.Description ?? string.Empty;

            // Fallback to first available translation (preferably English)
            var fallbackTranslation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                ?? department.Translations?.FirstOrDefault();

            return fallbackTranslation?.Description ?? string.Empty;
        }

        /// <summary>
        /// Converts Department entity to CreateDepartmentViewModel
        /// </summary>
        public static CreateDepartmentViewModel ToCreateViewModel(this Department department, Dictionary<string, string> supportedLanguages)
        {
            var viewModel = new CreateDepartmentViewModel
            {
                MainImg = department.MainImg,
                Status = department.Status
            };

            // Initialize translations for all supported languages
            foreach (var language in supportedLanguages)
            {
                var existingTranslation = department.Translations?.FirstOrDefault(t => 
                    t.LanguageCode.Equals(language.Key, StringComparison.OrdinalIgnoreCase));

                viewModel.Translations.Add(new DepartmentTranslationViewModel
                {
                    LanguageCode = language.Key,
                    LanguageName = language.Value,
                    Name = existingTranslation?.Name ?? string.Empty,
                    Description = existingTranslation?.Description ?? string.Empty
                });
            }

            return viewModel;
        }

        /// <summary>
        /// Converts Department entity to EditDepartmentViewModel
        /// </summary>
        public static EditDepartmentViewModel ToEditViewModel(this Department department, Dictionary<string, string> supportedLanguages)
        {
            var viewModel = new EditDepartmentViewModel
            {
                Id = department.Id,
                MainImg = department.MainImg,
                Status = department.Status
            };

            // Initialize translations for all supported languages
            foreach (var language in supportedLanguages)
            {
                var existingTranslation = department.Translations?.FirstOrDefault(t => 
                    t.LanguageCode.Equals(language.Key, StringComparison.OrdinalIgnoreCase));

                viewModel.Translations.Add(new DepartmentTranslationViewModel
                {
                    LanguageCode = language.Key,
                    LanguageName = language.Value,
                    Name = existingTranslation?.Name ?? string.Empty,
                    Description = existingTranslation?.Description ?? string.Empty
                });
            }

            return viewModel;
        }

        /// <summary>
        /// Converts CreateDepartmentViewModel to Department entity
        /// </summary>
        public static Department ToEntity(this CreateDepartmentViewModel viewModel, string tenantId)
        {
            var department = new Department
            {
                MainImg = viewModel.MainImg,
                Status = viewModel.Status,
                TenantId = tenantId
            };

            // Add translations
            foreach (var translation in viewModel.Translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)))
            {
                department.Translations.Add(new DepartmentTranslation
                {
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Description = translation.Description
                });
            }

            return department;
        }

        /// <summary>
        /// Updates Department entity from EditDepartmentViewModel
        /// </summary>
        public static void UpdateFromViewModel(this Department department, EditDepartmentViewModel viewModel)
        {
            department.MainImg = viewModel.MainImg;
            department.Status = viewModel.Status;

            // Clear existing translations
            department.Translations.Clear();

            // Add updated translations
            foreach (var translation in viewModel.Translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)))
            {
                department.Translations.Add(new DepartmentTranslation
                {
                    DepartmentId = department.Id,
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Description = translation.Description
                });
            }
        }

        /// <summary>
        /// Converts Department entity to DepartmentListViewModel with localized content
        /// </summary>
        public static DepartmentListViewModel ToListViewModel(this Department department, ILocalizationService localizationService)
        {
            return new DepartmentListViewModel
            {
                Id = department.Id,
                Name = localizationService.GetLocalizedDepartmentName(department),
                Description = localizationService.GetLocalizedDepartmentDescription(department),
                MainImg = department.MainImg,
                Status = department.Status,
                DoctorsCount = department.Doctors?.Count ?? 0,
                OriginalDepartment = department
            };
        }
    }
}

