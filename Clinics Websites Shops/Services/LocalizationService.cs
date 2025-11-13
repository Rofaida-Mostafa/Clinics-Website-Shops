using System.Globalization;
using Clinics_Websites_Shops.Services.IServices;

namespace Clinics_Websites_Shops.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        // Define supported languages - you can move this to configuration if needed
        private readonly Dictionary<string, string> _supportedLanguages = new()
        {
            { "en", "English" },
            { "ar", "العربية" }
        };

        public LocalizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetLocalizedDepartmentName(Department department, string? culture = null)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            // Use provided culture or get current culture
            var targetCulture = culture ?? GetCurrentCulture();

            // Try to find translation for the target culture
            var translation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.Equals(targetCulture, StringComparison.OrdinalIgnoreCase));

            // If translation found, return it
            if (translation != null)
                return translation.Name;

            // Fallback: try to get first available translation (preferably English)
            var fallbackTranslation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                ?? department.Translations?.FirstOrDefault();

            return fallbackTranslation?.Name ?? $"Department {department.Id}";
        }

        public string GetLocalizedDepartmentDescription(Department department, string? culture = null)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            // Use provided culture or get current culture
            var targetCulture = culture ?? GetCurrentCulture();

            // Try to find translation for the target culture
            var translation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.Equals(targetCulture, StringComparison.OrdinalIgnoreCase));

            // If translation found, return it
            if (translation != null)
                return translation.Description ?? string.Empty;

            // Fallback: try to get first available translation (preferably English)
            var fallbackTranslation = department.Translations?
                .FirstOrDefault(t => t.LanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                ?? department.Translations?.FirstOrDefault();

            return fallbackTranslation?.Description ?? string.Empty;
        }

        public string GetCurrentCulture()
        {
            // Try to get culture from current thread (set by localization middleware)
            var culture = CultureInfo.CurrentCulture.Name;

            // Fallback to route data if available
            if (_httpContextAccessor.HttpContext != null)
            {
                var routeCulture = _httpContextAccessor.HttpContext.GetRouteValue("culture")?.ToString();
                if (!string.IsNullOrEmpty(routeCulture))
                {
                    culture = routeCulture;
                }
            }

            return culture;
        }

        public Dictionary<string, string> GetSupportedLanguages()
        {
            return new Dictionary<string, string>(_supportedLanguages);
        }

        public string GetLanguageDisplayName(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return "Unknown";

            return _supportedLanguages.TryGetValue(languageCode, out var displayName) 
                ? displayName 
                : languageCode;
        }
    }
}

