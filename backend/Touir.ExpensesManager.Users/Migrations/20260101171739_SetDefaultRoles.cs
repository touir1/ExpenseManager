using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set IsDefault = true for the 'User' role under the app with Id 1 (EXPENSES_MANAGER)
            migrationBuilder.Sql(@"
                UPDATE ""RLE_Roles""
                SET ""RLE_IsDefault"" = TRUE
                WHERE ""RLE_Id"" = 2 AND ""RLE_ApplicationId"" = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert IsDefault to false for the 'User' role under the app with Id 1
            migrationBuilder.Sql(@"
                UPDATE ""RLE_Roles""
                SET ""RLE_IsDefault"" = FALSE
                WHERE ""RLE_Id"" = 2 AND ""RLE_ApplicationId"" = 1;
            ");
        }
    }
}
