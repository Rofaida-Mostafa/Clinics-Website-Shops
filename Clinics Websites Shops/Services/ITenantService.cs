namespace Clinics_Websites_Shops.Services
{
    public interface ITenantService
    {
        Tenant? GetCurrentTenant(HttpContext context);

   
    }
}
