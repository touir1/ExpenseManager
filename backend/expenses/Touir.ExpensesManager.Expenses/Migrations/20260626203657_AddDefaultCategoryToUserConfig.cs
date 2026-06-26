using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultCategoryToUserConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultCategoryId",
                table: "UserConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigs_DefaultCategoryId",
                table: "UserConfigs",
                column: "DefaultCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConfigs_Categories_DefaultCategoryId",
                table: "UserConfigs",
                column: "DefaultCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConfigs_Categories_DefaultCategoryId",
                table: "UserConfigs");

            migrationBuilder.DropIndex(
                name: "IX_UserConfigs_DefaultCategoryId",
                table: "UserConfigs");

            migrationBuilder.DropColumn(
                name: "DefaultCategoryId",
                table: "UserConfigs");
        }
    }
}
