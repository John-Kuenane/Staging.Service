using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "staging");

            migrationBuilder.CreateTable(
                name: "Package",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageTypeId = table.Column<int>(type: "int", nullable: false),
                    PackageOpened_Status = table.Column<bool>(type: "bit", nullable: true),
                    PackageOpened_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PackageClosed_Status = table.Column<bool>(type: "bit", nullable: true),
                    PackageClosed_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UniqueCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    System = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageEvent",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventOpened_Status = table.Column<bool>(type: "bit", nullable: true),
                    EventOpened_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceRegisteredStageClosed_Status = table.Column<bool>(type: "bit", nullable: true),
                    DeviceRegisteredStageClosed_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataManagementStageClosed_Status = table.Column<bool>(type: "bit", nullable: true),
                    DataManagementStageClosed_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataAcceptanceStageClosed_Status = table.Column<bool>(type: "bit", nullable: true),
                    DataAcceptanceStageClosed_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GatewayStageClosed_Status = table.Column<bool>(type: "bit", nullable: true),
                    GatewayStageClosed_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEvent_Package_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "staging",
                        principalTable: "Package",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventDataFlag",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HouseholdMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataFlagTypeId = table.Column<int>(type: "int", nullable: false),
                    SystemGenerated = table.Column<bool>(type: "bit", nullable: false),
                    FlagResolved_Status = table.Column<bool>(type: "bit", nullable: true),
                    FlagResolved_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlagDeferred_Status = table.Column<bool>(type: "bit", nullable: true),
                    FlagDeferred_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventDataFlag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventDataFlag_PackageEvent_PackageEventId",
                        column: x => x.PackageEventId,
                        principalSchema: "staging",
                        principalTable: "PackageEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventDevice",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EnumeratorName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventDevice_PackageEvent_PackageEventId",
                        column: x => x.PackageEventId,
                        principalSchema: "staging",
                        principalTable: "PackageEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventHousehold",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Village = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HouseholdHead = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Enumerated_Status = table.Column<bool>(type: "bit", nullable: true),
                    Enumerated_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Accepted_Status = table.Column<bool>(type: "bit", nullable: true),
                    Accepted_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rejected_Status = table.Column<bool>(type: "bit", nullable: true),
                    Rejected_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventHousehold", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventHousehold_PackageEvent_PackageEventId",
                        column: x => x.PackageEventId,
                        principalSchema: "staging",
                        principalTable: "PackageEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventDataFlagComment",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventDataFlagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventDataFlagComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventDataFlagComment_PackageEventDataFlag_PackageEventDataFlagId",
                        column: x => x.PackageEventDataFlagId,
                        principalSchema: "staging",
                        principalTable: "PackageEventDataFlag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventHouseholdAttribute",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Original_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Original_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueModified = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventHouseholdAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventHouseholdAttribute_PackageEventHousehold_PackageEventHouseholdId",
                        column: x => x.PackageEventHouseholdId,
                        principalSchema: "staging",
                        principalTable: "PackageEventHousehold",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventHouseholdMember",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IDDocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventHouseholdMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventHouseholdMember_PackageEventHousehold_PackageEventHouseholdId",
                        column: x => x.PackageEventHouseholdId,
                        principalSchema: "staging",
                        principalTable: "PackageEventHousehold",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventHouseholdSynch",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventHouseholdSynch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventHouseholdSynch_PackageEventHousehold_PackageEventHouseholdId",
                        column: x => x.PackageEventHouseholdId,
                        principalSchema: "staging",
                        principalTable: "PackageEventHousehold",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventHouseholdMemberAttribute",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Original_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Original_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueModified = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventHouseholdMemberAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventHouseholdMemberAttribute_PackageEventHouseholdMember_PackageEventHouseholdMemberId",
                        column: x => x.PackageEventHouseholdMemberId,
                        principalSchema: "staging",
                        principalTable: "PackageEventHouseholdMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventHouseholdSynchMetaAttribute",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetaAttributeTypeId = table.Column<int>(type: "int", nullable: false),
                    MetaAttributeValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdSynchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventHouseholdSynchMetaAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventHouseholdSynchMetaAttribute_PackageEventHouseholdSynch_PackageEventHouseholdSynchId",
                        column: x => x.PackageEventHouseholdSynchId,
                        principalSchema: "staging",
                        principalTable: "PackageEventHouseholdSynch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Package_UniqueCode",
                schema: "staging",
                table: "Package",
                column: "UniqueCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageEvent_PackageId",
                schema: "staging",
                table: "PackageEvent",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDataFlag_PackageEventId",
                schema: "staging",
                table: "PackageEventDataFlag",
                column: "PackageEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDataFlagComment_PackageEventDataFlagId",
                schema: "staging",
                table: "PackageEventDataFlagComment",
                column: "PackageEventDataFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDevice_PackageEventId",
                schema: "staging",
                table: "PackageEventDevice",
                column: "PackageEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHousehold_PackageEventId",
                schema: "staging",
                table: "PackageEventHousehold",
                column: "PackageEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdAttribute_PackageEventHouseholdId",
                schema: "staging",
                table: "PackageEventHouseholdAttribute",
                column: "PackageEventHouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdMember_PackageEventHouseholdId",
                schema: "staging",
                table: "PackageEventHouseholdMember",
                column: "PackageEventHouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdMemberAttribute_PackageEventHouseholdMemberId",
                schema: "staging",
                table: "PackageEventHouseholdMemberAttribute",
                column: "PackageEventHouseholdMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdSynch_PackageEventHouseholdId",
                schema: "staging",
                table: "PackageEventHouseholdSynch",
                column: "PackageEventHouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventHouseholdSynchMetaAttribute_PackageEventHouseholdSynchId",
                schema: "staging",
                table: "PackageEventHouseholdSynchMetaAttribute",
                column: "PackageEventHouseholdSynchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageEventDataFlagComment",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventDevice",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHouseholdAttribute",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHouseholdMemberAttribute",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHouseholdSynchMetaAttribute",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventDataFlag",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHouseholdMember",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHouseholdSynch",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHousehold",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEvent",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "Package",
                schema: "staging");
        }
    }
}
