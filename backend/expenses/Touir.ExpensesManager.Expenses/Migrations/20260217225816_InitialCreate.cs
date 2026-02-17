using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ext");

            migrationBuilder.CreateTable(
                name: "USR_Users",
                schema: "ext",
                columns: table => new
                {
                    USR_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USR_FirstName = table.Column<string>(type: "text", nullable: true),
                    USR_LastName = table.Column<string>(type: "text", nullable: true),
                    USR_Email = table.Column<string>(type: "text", nullable: true),
                    USR_FamilyId = table.Column<int>(type: "integer", nullable: true),
                    USR_IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USR_Users", x => x.USR_Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USR_Users",
                schema: "ext");
        }
    }
}
