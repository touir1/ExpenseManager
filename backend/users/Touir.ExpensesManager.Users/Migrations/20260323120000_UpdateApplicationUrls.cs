using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "APP_Applications",
                keyColumn: "APP_Id",
                keyValue: 1,
                columns: new[] { "APP_UrlPath", "APP_ResetPasswordUrlPath" },
                values: new object[] { "https://localhost", "https://localhost/reset-password" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "APP_Applications",
                keyColumn: "APP_Id",
                keyValue: 1,
                columns: new[] { "APP_UrlPath", "APP_ResetPasswordUrlPath" },
                values: new object[] { "http://localhost:5173", "http://localhost:5173/reset-password" });
        }
    }
}
