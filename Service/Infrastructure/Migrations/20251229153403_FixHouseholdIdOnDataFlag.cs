using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixHouseholdIdOnDataFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseholdId",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "HouseholdMemberId",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.AddColumn<int>(
                name: "PackageEventHouseholdId",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageEventHouseholdId",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdId",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdMemberId",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
