
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Services.IServices;

namespace Clinics_Websites_Shops.Services
{
   
        public class TenantService : ITenantService
        {
        private readonly MasterDbContext _masterDb;
        private readonly IConfiguration _configuration;

        public TenantService(MasterDbContext masterDb, IConfiguration configuration)
        {   
            _masterDb = masterDb;
            _configuration = configuration;
        }

        public Tenant? GetCurrentTenant(HttpContext context)
        {
            // نحاول نحدد tenant من الدومين
            var host = context.Request.Host.Host.ToLower();

            // detect by domain
            //return _masterDb.Tenants.FirstOrDefault(t => host.Contains(t.Domain.ToLower()) && t.Status);

            var tenant = _masterDb.Tenants
                .FirstOrDefault(t => host.Contains(t.Domain.ToLower()) && t.Status);

            if (tenant != null)
                return tenant;

            // fallback tenant (مؤقت)
            var fallbackTenantId = _configuration["TenantSettings:FallbackTenantId"];
            Console.WriteLine($"FallbackTenantId = {fallbackTenantId}");
            if (!string.IsNullOrEmpty(fallbackTenantId))
            {
                return _masterDb.Tenants
                    .FirstOrDefault(t => t.TId == fallbackTenantId && t.Status);
            }

            return null;
        }
        
        public Tenant? GetFirstTenant()
        {
            return _masterDb.Tenants.FirstOrDefault(); 
        }
    }
}
