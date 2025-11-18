using Microsoft.AspNetCore.Localization;

namespace Clinics_Websites_Shops.Middlewares
{
    public class RouteDataRequestCultureProvider : RequestCultureProvider
    {
        public int IndexOfCulture { get; set; }
        public int IndexofUICulture { get; set; }

        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext?.Request?.RouteValues == null)
                return Task.FromResult<ProviderCultureResult?>(null);

            var culture = httpContext.Request.RouteValues["culture"]?.ToString();
            
            if (string.IsNullOrEmpty(culture))
                return Task.FromResult<ProviderCultureResult?>(null);

            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
        }
    }
}

