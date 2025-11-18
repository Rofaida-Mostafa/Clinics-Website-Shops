namespace Clinics_Websites_Shops.Services.IServices
{
    public interface ITenantService
    {
        Tenant? GetCurrentTenant(HttpContext context);
        Tenant? GetFirstTenant();
   
    }
}
