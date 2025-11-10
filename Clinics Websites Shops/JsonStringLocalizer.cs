using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System.Text.Json;
using System.Globalization;
using System.Threading;

namespace Clinics_Websites_Shops;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly IDistributedCache _cache;
    private readonly JsonLocalizationOptions _options;

    public JsonStringLocalizer(IDistributedCache cache, JsonLocalizationOptions options)
    {
        _cache = cache;
        _options = options;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var actualValue = this[name];
            return !actualValue.ResourceNotFound
                ? new LocalizedString(name, string.Format(actualValue.Value, arguments))
                : actualValue;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var cultureName = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
        var filePath = $"Resources/{cultureName}.json";
        var fullFilePath = Path.GetFullPath(filePath);

        if (File.Exists(fullFilePath))
        {
            var jsonString = File.ReadAllText(fullFilePath);
            var jsonDocument = JsonDocument.Parse(jsonString);
            
            foreach (var property in jsonDocument.RootElement.EnumerateObject())
            {
                yield return new LocalizedString(property.Name, property.Value.GetString() ?? property.Name);
            }
          
        }
    }
            

    private string GetString(string key)
    {
        // Get the simplified culture code (e.g., "en" instead of "en-US")
        var cultureName = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
        var filePath = $"{_options.ResourcesPath}/{cultureName}.json";
        var fullFilePath = Path.GetFullPath(filePath);

        if (File.Exists(fullFilePath))
        {
            var cacheKey = $"locale_{cultureName}_{key}";
            var cacheValue = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;

            var result = GetValueFromJSON(key, fullFilePath);

            if (!string.IsNullOrEmpty(result))
                _cache.SetString(cacheKey, result);

            return result;
        }

        return string.Empty;
    }

    private string GetValueFromJSON(string key, string filePath)
    {
        try
        {
            var jsonString = File.ReadAllText(filePath);
            var jsonDocument = JsonDocument.Parse(jsonString);
            
            if (jsonDocument.RootElement.TryGetProperty(key, out var property))
            {
                return property.GetString() ?? key;
            }
        }
        catch (Exception)
        {
            // Log error if needed
        }
        
        return key; // Return key as fallback
    }
}
