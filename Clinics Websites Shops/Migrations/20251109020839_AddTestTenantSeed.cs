using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinics_Websites_Shops.Migrations.MasterDb
{
    /// <inheritdoc />
    public partial class AddTestTenantSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert test tenant data using raw SQL
            migrationBuilder.Sql(@"
                INSERT INTO Tenants (TId, Name, ConnectionString, Domain, Status, Locals, CreatedAt) 
                VALUES (
                    'test-clinic-001', 
                    'Test Clinic - Main Branch', 
                    'server=localhost;port=3306;database=ClinicsWebsiteShops;uid=root;pwd=;',
                    'localhost', 
                    1, 
                    'en,ar', 
                    UTC_TIMESTAMP()
                );");

            migrationBuilder.Sql(@"
                INSERT INTO Tenants (TId, Name, ConnectionString, Domain, Status, Locals, CreatedAt) 
                VALUES (
                    'test-clinic-002', 
                    'Test Clinic - Secondary Branch', 
                    'server=localhost;port=3306;database=ClinicsWebsiteShops_Test2;uid=root;pwd=;',
                    'test2.localhost', 
                    1, 
                    'en,ar', 
                    UTC_TIMESTAMP()
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove test tenant data using raw SQL
            migrationBuilder.Sql("DELETE FROM Tenants WHERE TId = 'test-clinic-001';");
            migrationBuilder.Sql("DELETE FROM Tenants WHERE TId = 'test-clinic-002';");
        }
    }
}
