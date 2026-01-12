using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFailureMessageToSynch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureMessage",
                schema: "staging",
                table: "PackageEventHouseholdSynch",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureMessage",
                schema: "staging",
                table: "PackageEventHouseholdSynch");
        }
    }
}
