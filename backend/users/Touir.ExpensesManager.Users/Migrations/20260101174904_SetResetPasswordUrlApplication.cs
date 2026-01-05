using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class SetResetPasswordUrlApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "APP_ResetPasswordUrlPath",
                table: "APP_Applications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "APP_Applications",
                keyColumn: "APP_Id",
                keyValue: 1,
                column: "APP_ResetPasswordUrlPath",
                value: "http://localhost:5173/reset-password");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "APP_ResetPasswordUrlPath",
                table: "APP_Applications");
        }
    }
}
