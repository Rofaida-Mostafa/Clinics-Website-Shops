using Microsoft.Extensions.Localization;

namespace Clinics_Websites_Shops;

public static class JsonLocalizationExtensions
{
    public static IServiceCollection AddJsonLocalization(this IServiceCollection services, Action<JsonLocalizationOptions> setupAction = null)
    {
        var options = new JsonLocalizationOptions();
        setupAction?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        
        return services;
    }
}

public class JsonLocalizationOptions
{
    public string ResourcesPath { get; set; } = "Resources";
}