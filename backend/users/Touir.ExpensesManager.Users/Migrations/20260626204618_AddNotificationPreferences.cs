using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NTF_NotificationPreferences",
                columns: table => new
                {
                    NTF_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    NTF_UserId = table.Column<int>(type: "integer", nullable: false),
                    NTF_EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NTF_EmailEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NTF_NotificationPreferences", x => x.NTF_Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NTF_NotificationPreferences_NTF_UserId_NTF_EventType",
                table: "NTF_NotificationPreferences",
                columns: new[] { "NTF_UserId", "NTF_EventType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NTF_NotificationPreferences");
        }
    }
}
