using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "USR_DeletedAt",
                table: "USR_Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "USR_IsDeleted",
                table: "USR_Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_usr_email_active ON \"USR_Users\" (\"USR_Email\") WHERE \"USR_IsDeleted\" = FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "USR_DeletedAt",
                table: "USR_Users");

            migrationBuilder.DropColumn(
                name: "USR_IsDeleted",
                table: "USR_Users");

            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_usr_email_active;");
        }
    }
}
