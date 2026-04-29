using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddVerifyEmailErrorUrlPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "APP_VerifyEmailErrorUrlPath",
                table: "APP_Applications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "APP_Applications",
                keyColumn: "APP_Id",
                keyValue: 1,
                column: "APP_VerifyEmailErrorUrlPath",
                value: "/verify-error");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "APP_VerifyEmailErrorUrlPath",
                table: "APP_Applications");
        }
    }
}
