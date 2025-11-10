using Microsoft.EntityFrameworkCore;
using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.Settings;

namespace Clinics_Websites_Shops.Services
{
    public class TenantDatabaseInitializer
    {
        public async Task InitializeDatabaseAsync(Tenant tenant)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(tenant.ConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                //  ينشئ قاعدة البيانات لو مش موجودة
                await context.Database.MigrateAsync();
            }
        }
    }
}
