using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class SchemaFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConflictResolutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConflictResolutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConflictStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConflictStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Decimals = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Families_USR_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModifiedSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModifiedSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RateSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SnapshotTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_USR_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyPairDefaults",
                columns: table => new
                {
                    SourceCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    DestinationCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyPairDefaults", x => new { x.SourceCurrencyId, x.DestinationCurrencyId });
                    table.ForeignKey(
                        name: "FK_CurrencyPairDefaults_Currencies_DestinationCurrencyId",
                        column: x => x.DestinationCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyPairDefaults_Currencies_SourceCurrencyId",
                        column: x => x.SourceCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRateConflicts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    DestinationCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    AutomaticRate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    ManualRate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedById = table.Column<int>(type: "integer", nullable: true),
                    ResolutionId = table.Column<int>(type: "integer", nullable: true),
                    CustomRate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRateConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyRateConflicts_ConflictResolutions_ResolutionId",
                        column: x => x.ResolutionId,
                        principalTable: "ConflictResolutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyRateConflicts_ConflictStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ConflictStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyRateConflicts_Currencies_DestinationCurrencyId",
                        column: x => x.DestinationCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyRateConflicts_Currencies_SourceCurrencyId",
                        column: x => x.SourceCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyRateConflicts_USR_Users_ResolvedById",
                        column: x => x.ResolvedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FamilyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyMemberships_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyMemberships_FamilyRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "FamilyRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyMemberships_USR_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedFromId = table.Column<int>(type: "integer", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<int>(type: "integer", nullable: true),
                    ModifiedFromId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Categories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_ModifiedSources_ModifiedFromId",
                        column: x => x.ModifiedFromId,
                        principalTable: "ModifiedSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_OperationSources_CreatedFromId",
                        column: x => x.CreatedFromId,
                        principalTable: "OperationSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_USR_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_USR_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_USR_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyDailyRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    DestinationCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    RateSourceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyDailyRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyDailyRates_Currencies_DestinationCurrencyId",
                        column: x => x.DestinationCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyDailyRates_Currencies_SourceCurrencyId",
                        column: x => x.SourceCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyDailyRates_RateSources_RateSourceId",
                        column: x => x.RateSourceId,
                        principalTable: "RateSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpenseId = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<int>(type: "integer", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PerformedById = table.Column<int>(type: "integer", nullable: false),
                    PerformedFromId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditLogs_AuditOperations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "AuditOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditLogs_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditLogs_OperationSources_PerformedFromId",
                        column: x => x.PerformedFromId,
                        principalTable: "OperationSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditLogs_USR_Users_PerformedById",
                        column: x => x.PerformedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseFamilyAttributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpenseId = table.Column<int>(type: "integer", nullable: false),
                    FamilyId = table.Column<int>(type: "integer", nullable: false),
                    AttributedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttributedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseFamilyAttributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseFamilyAttributions_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseFamilyAttributions_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseFamilyAttributions_USR_Users_AttributedById",
                        column: x => x.AttributedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseTags",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseTags", x => new { x.ExpenseId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ExpenseTags_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseAuditSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditLogId = table.Column<int>(type: "integer", nullable: false),
                    SnapshotTypeId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Families = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseAuditSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditSnapshots_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditSnapshots_ExpenseAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "ExpenseAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseAuditSnapshots_SnapshotTypes_SnapshotTypeId",
                        column: x => x.SnapshotTypeId,
                        principalTable: "SnapshotTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AuditOperations",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Add" },
                    { 2, "Update" },
                    { 3, "Delete" }
                });

            migrationBuilder.InsertData(
                table: "ConflictResolutions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "AcceptAuto" },
                    { 2, "KeepManual" },
                    { 3, "Custom" }
                });

            migrationBuilder.InsertData(
                table: "ConflictStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Resolved" }
                });

            migrationBuilder.InsertData(
                table: "FamilyRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Head" },
                    { 2, "Member" }
                });

            migrationBuilder.InsertData(
                table: "ModifiedSources",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Web" },
                    { 2, "Mobile" }
                });

            migrationBuilder.InsertData(
                table: "OperationSources",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "SingleWeb" },
                    { 2, "SingleMobile" },
                    { 3, "BulkWeb" }
                });

            migrationBuilder.InsertData(
                table: "RateSources",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Auto" },
                    { 2, "Manual" }
                });

            migrationBuilder.InsertData(
                table: "SnapshotTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Before" },
                    { 2, "After" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditOperations_Name",
                table: "AuditOperations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictResolutions_Name",
                table: "ConflictResolutions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConflictStatuses_Name",
                table: "ConflictStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyDailyRates_DestinationCurrencyId",
                table: "CurrencyDailyRates",
                column: "DestinationCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyDailyRates_RateSourceId",
                table: "CurrencyDailyRates",
                column: "RateSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyDailyRates_SourceCurrencyId_DestinationCurrencyId_D~",
                table: "CurrencyDailyRates",
                columns: new[] { "SourceCurrencyId", "DestinationCurrencyId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyPairDefaults_DestinationCurrencyId",
                table: "CurrencyPairDefaults",
                column: "DestinationCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateConflicts_DestinationCurrencyId",
                table: "CurrencyRateConflicts",
                column: "DestinationCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateConflicts_ResolutionId",
                table: "CurrencyRateConflicts",
                column: "ResolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateConflicts_ResolvedById",
                table: "CurrencyRateConflicts",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateConflicts_SourceCurrencyId",
                table: "CurrencyRateConflicts",
                column: "SourceCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateConflicts_StatusId",
                table: "CurrencyRateConflicts",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditLogs_ExpenseId",
                table: "ExpenseAuditLogs",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditLogs_OperationId",
                table: "ExpenseAuditLogs",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditLogs_PerformedById",
                table: "ExpenseAuditLogs",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditLogs_PerformedFromId",
                table: "ExpenseAuditLogs",
                column: "PerformedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditSnapshots_AuditLogId",
                table: "ExpenseAuditSnapshots",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditSnapshots_CurrencyId",
                table: "ExpenseAuditSnapshots",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAuditSnapshots_SnapshotTypeId",
                table: "ExpenseAuditSnapshots",
                column: "SnapshotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseFamilyAttributions_AttributedById",
                table: "ExpenseFamilyAttributions",
                column: "AttributedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseFamilyAttributions_ExpenseId",
                table: "ExpenseFamilyAttributions",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseFamilyAttributions_FamilyId",
                table: "ExpenseFamilyAttributions",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseTags_TagId",
                table: "ExpenseTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedById",
                table: "Expenses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedFromId",
                table: "Expenses",
                column: "CreatedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CurrencyId",
                table: "Expenses",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ModifiedById",
                table: "Expenses",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ModifiedFromId",
                table: "Expenses",
                column: "ModifiedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SubcategoryId",
                table: "Expenses",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId_Date",
                table: "Expenses",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Families_CreatedById",
                table: "Families",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_FamilyId",
                table: "FamilyMemberships",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_RoleId",
                table: "FamilyMemberships",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_UserId",
                table: "FamilyMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRoles_Name",
                table: "FamilyRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModifiedSources_Name",
                table: "ModifiedSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationSources_Name",
                table: "OperationSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RateSources_Name",
                table: "RateSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotTypes_Name",
                table: "SnapshotTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId",
                table: "Tags",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyDailyRates");

            migrationBuilder.DropTable(
                name: "CurrencyPairDefaults");

            migrationBuilder.DropTable(
                name: "CurrencyRateConflicts");

            migrationBuilder.DropTable(
                name: "ExpenseAuditSnapshots");

            migrationBuilder.DropTable(
                name: "ExpenseFamilyAttributions");

            migrationBuilder.DropTable(
                name: "ExpenseTags");

            migrationBuilder.DropTable(
                name: "FamilyMemberships");

            migrationBuilder.DropTable(
                name: "RateSources");

            migrationBuilder.DropTable(
                name: "ConflictResolutions");

            migrationBuilder.DropTable(
                name: "ConflictStatuses");

            migrationBuilder.DropTable(
                name: "ExpenseAuditLogs");

            migrationBuilder.DropTable(
                name: "SnapshotTypes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropTable(
                name: "FamilyRoles");

            migrationBuilder.DropTable(
                name: "AuditOperations");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "ModifiedSources");

            migrationBuilder.DropTable(
                name: "OperationSources");
        }
    }
}
