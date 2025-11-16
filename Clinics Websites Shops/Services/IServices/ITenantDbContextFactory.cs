using Clinics_Websites_Shops.DataAccess;

namespace Clinics_Websites_Shops.Services.IServices
{
    public interface ITenantDbContextFactory
    {
            ApplicationDbContext CreateDbContext();

    }
}
