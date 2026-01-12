using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportFieldsToDataFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "nvarchar(750)",
                maxLength: 750,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "DataFlagSubTypeId",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Requester_Email",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requester_FullName",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFlagSubTypeId",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "GroupId",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "Requester_Email",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "Requester_FullName",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.DropColumn(
                name: "Subject",
                schema: "staging",
                table: "PackageEventDataFlag");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "staging",
                table: "PackageEventDataFlag",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(750)",
                oldMaxLength: 750);
        }
    }
}
