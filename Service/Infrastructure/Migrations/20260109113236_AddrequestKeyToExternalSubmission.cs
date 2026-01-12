using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddrequestKeyToExternalSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestKey",
                schema: "staging",
                table: "ExternalSubmissionItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSubmissionItems_Vendor_Type_RequestKey",
                schema: "staging",
                table: "ExternalSubmissionItems",
                columns: new[] { "Vendor", "Type", "RequestKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExternalSubmissionItems_Vendor_Type_RequestKey",
                schema: "staging",
                table: "ExternalSubmissionItems");

            migrationBuilder.DropColumn(
                name: "RequestKey",
                schema: "staging",
                table: "ExternalSubmissionItems");
        }
    }
}
