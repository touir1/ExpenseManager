using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DefaultCurrencyId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConfigs_Currencies_DefaultCurrencyId",
                        column: x => x.DefaultCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigs_DefaultCurrencyId",
                table: "UserConfigs",
                column: "DefaultCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigs_UserId",
                table: "UserConfigs",
                column: "UserId",
                unique: true);

            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🏠' WHERE \"Id\" = 1");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🍽️' WHERE \"Id\" = 2");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🚗' WHERE \"Id\" = 3");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🏥' WHERE \"Id\" = 4");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🛍️' WHERE \"Id\" = 5");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '💆' WHERE \"Id\" = 6");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '📄' WHERE \"Id\" = 7");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '📚' WHERE \"Id\" = 8");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '👨‍👩‍👧' WHERE \"Id\" = 9");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🎭' WHERE \"Id\" = 10");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '✈️' WHERE \"Id\" = 11");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🐾' WHERE \"Id\" = 12");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '🎁' WHERE \"Id\" = 13");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '💳' WHERE \"Id\" = 14");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '💼' WHERE \"Id\" = 15");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '⚖️' WHERE \"Id\" = 16");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"Icon\" = '📦' WHERE \"Id\" = 17");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserConfigs");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Categories");
        }
    }
}
