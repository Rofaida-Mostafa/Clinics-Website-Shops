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

            // Return translated name if found, otherwise return default name
            return translation?.Name ?? String.Empty;
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

            // Return translated description if found, otherwise return default description
            return translation?.Description ?? String.Empty;
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

