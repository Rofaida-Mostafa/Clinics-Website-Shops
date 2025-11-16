using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Services.IServices;

namespace Clinics_Websites_Shops.Services
{
    public class TenantDbContextFactory : ITenantDbContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantDbContextFactory(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var tenant = _httpContextAccessor.HttpContext?.Items["Tenant"] as Tenant;

            if (tenant == null)
                throw new Exception("No tenant found for this domain.");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(tenant.ConnectionString)
                .Options;

            return new ApplicationDbContext(options);
        }
    }

}
