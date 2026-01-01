using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "APP_Applications",
                columns: new[] { "APP_Id", "APP_Code", "APP_Name", "APP_Description", "APP_UrlPath" },
                values: new object[] { 1, "EXPENSES_MANAGER", "Expenses Manager", "The expenses manager application", "http://localhost:5173" }
            );

            migrationBuilder.InsertData(
                table: "RLE_Roles",
                columns: new[] { "RLE_Id", "RLE_Code", "RLE_Name", "RLE_Description", "RLE_ApplicationId" },
                values: new object[,]
                {
                    { 1, "ADMIN", "Administrator", "Administrator role with full access", 1 },
                    { 2, "USER", "User", "Standard user role with limited access", 1 }
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RLE_Roles",
                keyColumn: "RLE_Id",
                keyValue: 1
            );
            migrationBuilder.DeleteData(
                table: "RLE_Roles",
                keyColumn: "RLE_Id",
                keyValue: 2
            );
            migrationBuilder.DeleteData(
                table: "APP_Applications",
                keyColumn: "APP_Id",
                keyValue: 1
            );
        }
    }
}
