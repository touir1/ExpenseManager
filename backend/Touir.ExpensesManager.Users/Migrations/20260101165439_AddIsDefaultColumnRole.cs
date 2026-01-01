using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDefaultColumnRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RLE_IsDefault",
                table: "RLE_Roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RLE_IsDefault",
                table: "RLE_Roles");
        }
    }
}
