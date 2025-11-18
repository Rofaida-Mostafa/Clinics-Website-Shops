using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;

namespace Clinics_Websites_Shops;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IDistributedCache _cache;
    private readonly JsonLocalizationOptions _options;

    public JsonStringLocalizerFactory(IDistributedCache cache, JsonLocalizationOptions options)
    {
        _cache = cache;
        _options = options;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer(_cache, _options);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer(_cache, _options);
    }
}