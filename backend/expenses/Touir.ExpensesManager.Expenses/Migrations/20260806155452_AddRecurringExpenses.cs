using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurrenceFrequencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrenceFrequencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecurringExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: true),
                    FamilyId = table.Column<int>(type: "integer", nullable: true),
                    FrequencyId = table.Column<int>(type: "integer", nullable: false),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_Categories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_RecurrenceFrequencies_FrequencyId",
                        column: x => x.FrequencyId,
                        principalTable: "RecurrenceFrequencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RecurrenceFrequencies",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Weekly" },
                    { 2, "Monthly" },
                    { 3, "Yearly" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceFrequencies_Name",
                table: "RecurrenceFrequencies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_CategoryId",
                table: "RecurringExpenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_CurrencyId",
                table: "RecurringExpenses",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_FamilyId",
                table: "RecurringExpenses",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_FrequencyId",
                table: "RecurringExpenses",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_SubcategoryId",
                table: "RecurringExpenses",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_UserId_IsActive_NextDueDate",
                table: "RecurringExpenses",
                columns: new[] { "UserId", "IsActive", "NextDueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecurringExpenses");

            migrationBuilder.DropTable(
                name: "RecurrenceFrequencies");
        }
    }
}
