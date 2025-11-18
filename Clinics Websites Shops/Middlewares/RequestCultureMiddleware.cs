namespace Clinics_Websites_Shops.Middlewares;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
public class RequestCultureMiddleware
{
    private readonly RequestDelegate _next;

    public RequestCultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var currentLanguage = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        var browserLanguage = context.Request.Headers["Accept-Language"].ToString()[..2];

        if (string.IsNullOrEmpty(currentLanguage))
        {
            var culture = string.Empty;

            switch (browserLanguage)
            {
                case "ar":
                    culture = "ar";
                    break;

                case "de":
                    culture = "de";
                    break;

                default:
                    culture = "en";
                    break;
            }

            var requestCulture = new RequestCulture(culture, culture);
            context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, null));

            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
        }

        await _next(context);
    }
}