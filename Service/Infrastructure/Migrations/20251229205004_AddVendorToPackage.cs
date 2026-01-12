using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorToPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Vendor",
                schema: "staging",
                table: "Package",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vendor",
                schema: "staging",
                table: "Package");
        }
    }
}
