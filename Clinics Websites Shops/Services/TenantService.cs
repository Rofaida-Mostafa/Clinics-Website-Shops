
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Services.IServices;

namespace Clinics_Websites_Shops.Services
{
   
        public class TenantService : ITenantService
        {
        private readonly MasterDbContext _masterDb;

        public TenantService(MasterDbContext masterDb)
        {
            _masterDb = masterDb;
        }

        public Tenant? GetCurrentTenant(HttpContext context)
        {
            var host = context.Request.Host.Host.ToLower();

            // detect by domain
            return _masterDb.Tenants.FirstOrDefault(t => host.Contains(t.Domain.ToLower()) && t.Status);
        }
        
        public Tenant? GetFirstTenant()
        {
            return _masterDb.Tenants.FirstOrDefault(); 
        }
    }
}
