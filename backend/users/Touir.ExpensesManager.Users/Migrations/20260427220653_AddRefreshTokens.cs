using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RTK_RefreshTokens",
                columns: table => new
                {
                    RTK_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    RTK_Token = table.Column<string>(type: "text", nullable: false),
                    RTK_UserId = table.Column<int>(type: "integer", nullable: false),
                    RTK_ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RTK_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RTK_RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RTK_RefreshTokens", x => x.RTK_Id);
                    table.ForeignKey(
                        name: "FK_RTK_RefreshTokens_USR_Users_RTK_UserId",
                        column: x => x.RTK_UserId,
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RTK_RefreshTokens_RTK_Token",
                table: "RTK_RefreshTokens",
                column: "RTK_Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RTK_RefreshTokens_RTK_UserId",
                table: "RTK_RefreshTokens",
                column: "RTK_UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RTK_RefreshTokens");
        }
    }
}
