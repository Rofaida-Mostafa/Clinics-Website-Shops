using Clinics_Websites_Shops.Services.IServices;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public TenantMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

        var tenant = tenantService.GetCurrentTenant(context);

        if (tenant != null)
            context.Items["Tenant"] = tenant;

        await _next(context);
    }
}
