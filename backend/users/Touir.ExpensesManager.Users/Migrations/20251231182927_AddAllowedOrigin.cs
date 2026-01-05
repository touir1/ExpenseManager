using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ALW_AllowedOrigins",
                columns: table => new
                {
                    ALW_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ALW_Url = table.Column<string>(type: "text", nullable: false),
                    ALW_IsGlobal = table.Column<bool>(type: "boolean", nullable: false),
                    ALW_ApplicationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALW_AllowedOrigins", x => x.ALW_Id);
                    table.ForeignKey(
                        name: "FK_ALW_AllowedOrigins_APP_Applications_ALW_ApplicationId",
                        column: x => x.ALW_ApplicationId,
                        principalTable: "APP_Applications",
                        principalColumn: "APP_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ALW_AllowedOrigins_ALW_ApplicationId",
                table: "ALW_AllowedOrigins",
                column: "ALW_ApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALW_AllowedOrigins");
        }
    }
}
