using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentityVerificationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageEventHouseholdMember_IdentityVerificationStatus",
                schema: "staging",
                table: "PackageEventHouseholdMember");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdMember_IdentityVerificationStatus",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                column: "IdentityVerification_Status");
        }
    }
}
