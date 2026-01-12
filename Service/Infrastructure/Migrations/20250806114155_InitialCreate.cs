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
                name: "Form",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FriendlyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UniqueCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Help = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FormStatusId = table.Column<int>(type: "int", nullable: false),
                    CurrentVersion_Major = table.Column<int>(type: "int", nullable: true),
                    CurrentVersion_Minor = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageTypeId = table.Column<int>(type: "int", nullable: false),
                    PackageOpened_Status = table.Column<bool>(type: "bit", nullable: true),
                    PackageOpened_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PackageClosed_Status = table.Column<bool>(type: "bit", nullable: true),
                    PackageClosed_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentOrgunitName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UniqueCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    System = table.Column<bool>(type: "bit", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: true),
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
                name: "FormCategory",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FriendlyName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Help = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    VersionCreated_Major = table.Column<int>(type: "int", nullable: true),
                    VersionCreated_Minor = table.Column<int>(type: "int", nullable: true),
                    VersionModified_Major = table.Column<int>(type: "int", nullable: true),
                    VersionModified_Minor = table.Column<int>(type: "int", nullable: true),
                    VersionDeleted_Major = table.Column<int>(type: "int", nullable: true),
                    VersionDeleted_Minor = table.Column<int>(type: "int", nullable: true),
                    ExtendableTypeName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormId = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormCategory_Form_FormId",
                        column: x => x.FormId,
                        principalSchema: "staging",
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormVersion",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version_Major = table.Column<int>(type: "int", nullable: true),
                    Version_Minor = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormId = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormVersion_Form_FormId",
                        column: x => x.FormId,
                        principalSchema: "staging",
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEvent",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageStatusId = table.Column<int>(type: "int", nullable: false),
                    PackageSubStatusId = table.Column<int>(type: "int", nullable: true),
                    StagePreparationClosed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataManagementStageClosed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataAcceptanceStageClosed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GatewayStageClosed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgUnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastTemporaryHouseholdId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageId = table.Column<int>(type: "int", nullable: true)
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
                name: "FormElement",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElementTypeId = table.Column<int>(type: "int", nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FriendlyName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Help = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CustomAttributeConfigurationGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomAttributeTypeId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: true),
                    AttributeKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AttributeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    StringMaxLength = table.Column<int>(type: "int", nullable: true),
                    NumericMinValue = table.Column<int>(type: "int", nullable: true),
                    NumericMaxValue = table.Column<int>(type: "int", nullable: true),
                    FutureDateOnly = table.Column<bool>(type: "bit", nullable: false),
                    PastDateOnly = table.Column<bool>(type: "bit", nullable: false),
                    MultipleSelection = table.Column<bool>(type: "bit", nullable: false),
                    RegEx = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsFormula = table.Column<bool>(type: "bit", nullable: false),
                    VersionCreated_Major = table.Column<int>(type: "int", nullable: true),
                    VersionCreated_Minor = table.Column<int>(type: "int", nullable: true),
                    VersionModified_Major = table.Column<int>(type: "int", nullable: true),
                    VersionModified_Minor = table.Column<int>(type: "int", nullable: true),
                    VersionDeleted_Major = table.Column<int>(type: "int", nullable: true),
                    VersionDeleted_Minor = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormCategoryId = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormElement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormElement_FormCategory_FormCategoryId",
                        column: x => x.FormCategoryId,
                        principalSchema: "staging",
                        principalTable: "FormCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventDataFlag",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    PackageEventId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EnumeratorName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdId = table.Column<int>(type: "int", nullable: false),
                    HouseholdGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommunityClassification = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HouseholdHead = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    PhysicalAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VillageName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ListingStatusId = table.Column<int>(type: "int", nullable: false),
                    ListingStatusDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CollectionStatusId = table.Column<int>(type: "int", nullable: false),
                    CollectionStatusDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Accepted_Status = table.Column<bool>(type: "bit", nullable: true),
                    Accepted_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rejected_Status = table.Column<bool>(type: "bit", nullable: true),
                    Rejected_ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventId = table.Column<int>(type: "int", nullable: true)
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
                name: "FormElementAttribute",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormElementId = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormElementAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormElementAttribute_FormElement_FormElementId",
                        column: x => x.FormElementId,
                        principalSchema: "staging",
                        principalTable: "FormElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormElementDependency",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComparisonFormElementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependencyRelationshipTypeId = table.Column<int>(type: "int", nullable: false),
                    OperatorTypeId = table.Column<int>(type: "int", nullable: false),
                    ComparisonValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormElementId = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormElementDependency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormElementDependency_FormElement_FormElementId",
                        column: x => x.FormElementId,
                        principalSchema: "staging",
                        principalTable: "FormElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventDataFlagComment",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventDataFlagId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Original_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Original_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Original_Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueModified = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdMemberId = table.Column<int>(type: "int", nullable: false),
                    HouseholdMemberGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IDDocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayloadProcessedId = table.Column<int>(type: "int", nullable: false),
                    PayloadProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Original_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Original_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Original_Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_SelectionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified_Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueModified = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdMemberId = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetaAttributeTypeId = table.Column<int>(type: "int", nullable: false),
                    MetaAttributeValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageEventHouseholdSynchId = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_Form_ShortName",
                schema: "staging",
                table: "Form",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormCategory_FormId",
                schema: "staging",
                table: "FormCategory",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormCategory_ShortName",
                schema: "staging",
                table: "FormCategory",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_FormElement_FormCategoryId",
                schema: "staging",
                table: "FormElement",
                column: "FormCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FormElement_ShortName",
                schema: "staging",
                table: "FormElement",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_FormElementAttribute_FormElementId",
                schema: "staging",
                table: "FormElementAttribute",
                column: "FormElementId");

            migrationBuilder.CreateIndex(
                name: "IX_FormElementDependency_FormElementId",
                schema: "staging",
                table: "FormElementDependency",
                column: "FormElementId");

            migrationBuilder.CreateIndex(
                name: "IX_FormVersion_FormId",
                schema: "staging",
                table: "FormVersion",
                column: "FormId");

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
                name: "FormElementAttribute",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "FormElementDependency",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "FormVersion",
                schema: "staging");

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
                name: "FormElement",
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
                name: "FormCategory",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "PackageEventHousehold",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "Form",
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
