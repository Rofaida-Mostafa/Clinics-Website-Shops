using Clinics_Websites_Shops.DataAccess;
using Microsoft.EntityFrameworkCore.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Clinics_Websites_Shops.DataAccess
{
        public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
        {
            public MasterDbContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
                optionsBuilder.UseSqlServer("Server=.;Database=MasterDb;Trusted_Connection=True;TrustServerCertificate=True;");

                return new MasterDbContext(optionsBuilder.Options);
            }
        }

}
