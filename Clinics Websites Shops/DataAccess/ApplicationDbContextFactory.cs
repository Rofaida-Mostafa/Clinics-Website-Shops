using Clinics_Websites_Shops.DataAccess.Extensions;
using Clinics_Websites_Shops.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinics_Websites_Shops.DataAccess
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        // CreateDbContext is a method that EF Core calls when executing commands like Add-Migration.
        // It should return an object of ApplicationDbContext.
        // Inside it, we define the database connection settings (connection string) and its type.

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Create EnvironmentService to read .env configuration
            var environmentService = new EnvironmentService();
            
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use dynamic configuration based on .env file
            var connectionString = environmentService.GetConnectionString();
            var databaseProvider = environmentService.GetDatabaseProvider();
            
            // Configure database with the detected provider
            optionsBuilder.ConfigureDatabase(connectionString, databaseProvider);
        
            // When EF Core runs the factory (e.g., during Add-Migration), it doesn't have access to dependency injection,
            // so it can't provide services like ITenantService or IHttpContextAccessor.
            // That's why developers pass null! as a placeholder for these services during design-time operations.
            // The '!' is the null-forgiving operator, telling the compiler to ignore nullable warnings for this null value.
            // The EnvironmentService is provided since it's needed for database configuration.

            return new ApplicationDbContext(optionsBuilder.Options, null!, null!, environmentService);
        }
    }
}