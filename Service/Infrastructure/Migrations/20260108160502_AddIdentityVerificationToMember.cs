using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityVerificationToMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnumeratedIdentity_DateOfBirth",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnumeratedIdentity_FirstName",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnumeratedIdentity_IdentificationNumber",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnumeratedIdentity_Surname",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityVerification_Message",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IdentityVerification_RecordedAt",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityVerification_Reference",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityVerification_Status",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "IdentityVerification_VerifiedIdentity_DateOfBirth",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityVerification_VerifiedIdentity_FirstName",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityVerification_VerifiedIdentity_IdentificationNumber",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityVerification_VerifiedIdentity_Surname",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "staging",
                table: "ExternalSubmissionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdMember_IdentityVerificationStatus",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                column: "IdentityVerification_Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageEventHouseholdMember_IdentityVerificationStatus",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "EnumeratedIdentity_DateOfBirth",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "EnumeratedIdentity_FirstName",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "EnumeratedIdentity_IdentificationNumber",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "EnumeratedIdentity_Surname",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_Message",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_RecordedAt",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_Reference",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_Status",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_VerifiedIdentity_DateOfBirth",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_VerifiedIdentity_FirstName",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_VerifiedIdentity_IdentificationNumber",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "IdentityVerification_VerifiedIdentity_Surname",
                schema: "staging",
                table: "PackageEventHouseholdMember");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "staging",
                table: "ExternalSubmissionItems");
        }
    }
}
