namespace Clinics_Websites_Shops.DataAccess
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        // CreateDbContext is a method that EF Core calls when executing commands like Add-Migration.
        // It should return an object of ApplicationDbContext.
        // Inside it, we define the database connection settings (connection string) and its type.

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer("Server=.;Database=ClinicOneDb;Trusted_Connection=True;TrustServerCertificate=True;");
        
            // When EF Core runs the factory (e.g., during Add-Migration), it doesn’t have access to dependency injection,
            // so it can’t provide services like ITenantService or ILogger.
            // That’s why developers pass null! as a placeholder.
            // The '!' is the null-forgiving operator, telling the compiler to ignore nullable warnings for this null value.

            return new ApplicationDbContext(optionsBuilder.Options, null!, null!);
        }
    }
}
