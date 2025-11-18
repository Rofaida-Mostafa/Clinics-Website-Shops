using Clinics_Websites_Shops.DataAccess;
using Clinics_Websites_Shops.DataAccess.Extensions;
using Clinics_Websites_Shops.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinics_Websites_Shops.DataAccess
{
    public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
    {
        public MasterDbContext CreateDbContext(string[] args)
        {
            // Create EnvironmentService to read .env configuration
            var environmentService = new EnvironmentService();
            
            var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
            
            // Use dynamic configuration based on .env file
            var connectionString = environmentService.GetMasterConnectionString();
            var databaseProvider = environmentService.GetDatabaseProvider();
            
            // Configure database with the detected provider
            optionsBuilder.ConfigureDatabase(connectionString, databaseProvider);

            return new MasterDbContext(optionsBuilder.Options, environmentService);
        }
    }

}
