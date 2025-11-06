using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinics_Websites_Shops.Migrations.Master
{
    /// <inheritdoc />
    public partial class addLocals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Locals",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locals",
                table: "Tenants");
        }
    }
}
