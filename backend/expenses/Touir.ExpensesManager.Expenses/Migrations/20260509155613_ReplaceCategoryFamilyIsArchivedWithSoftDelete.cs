using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCategoryFamilyIsArchivedWithSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Families",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Families",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE \"Families\" SET \"IsDeleted\" = \"IsArchived\", \"DeletedAt\" = CASE WHEN \"IsArchived\" THEN CURRENT_TIMESTAMP ELSE NULL END;");

            migrationBuilder.Sql(
                "UPDATE \"Categories\" SET \"IsDeleted\" = \"IsArchived\", \"DeletedAt\" = CASE WHEN \"IsArchived\" THEN CURRENT_TIMESTAMP ELSE NULL END;");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Families",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
