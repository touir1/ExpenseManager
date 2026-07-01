using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultCsvColumnMappingToUserConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultCsvColumnMappingJson",
                table: "UserConfigs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultCsvColumnMappingJson",
                table: "UserConfigs");
        }
    }
}
