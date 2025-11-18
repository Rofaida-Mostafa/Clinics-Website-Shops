namespace Clinics_Websites_Shops.Services.IServices
{
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the localized name for a department based on the current culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Optional culture code (e.g., "en-US", "ar-EG"). If null, uses current culture.</param>
        /// <returns>Localized department name or default name if translation not found</returns>
        string GetLocalizedDepartmentName(Department department, string? culture = null);
        
        /// <summary>
        /// Gets the localized description for a department based on the current culture
        /// </summary>
        /// <param name="department">The department entity</param>
        /// <param name="culture">Optional culture code (e.g., "en-US", "ar-EG"). If null, uses current culture.</param>
        /// <returns>Localized department description or default description if translation not found</returns>
        string GetLocalizedDepartmentDescription(Department department, string? culture = null);
        
        /// <summary>
        /// Gets the current culture code from the request
        /// </summary>
        /// <returns>Current culture code (e.g., "en-US")</returns>
        string GetCurrentCulture();
        
        /// <summary>
        /// Gets all supported languages with their display names
        /// </summary>
        /// <returns>Dictionary of language codes and display names</returns>
        Dictionary<string, string> GetSupportedLanguages();
        
        /// <summary>
        /// Gets the display name for a language code
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en-US", "ar-EG")</param>
        /// <returns>Display name of the language</returns>
        string GetLanguageDisplayName(string languageCode);
    }
}

