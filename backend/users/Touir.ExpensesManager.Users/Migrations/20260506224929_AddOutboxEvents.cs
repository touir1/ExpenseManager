using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MSG_OutboxEvents",
                columns: table => new
                {
                    MSG_Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    MSG_MessageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    MSG_EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MSG_Payload = table.Column<string>(type: "text", nullable: false),
                    MSG_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MSG_PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MSG_RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MSG_LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSG_OutboxEvents", x => x.MSG_Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MSG_OutboxEvents_MSG_MessageId",
                table: "MSG_OutboxEvents",
                column: "MSG_MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MSG_OutboxEvents_MSG_PublishedAt_MSG_RetryCount",
                table: "MSG_OutboxEvents",
                columns: new[] { "MSG_PublishedAt", "MSG_RetryCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MSG_OutboxEvents");
        }
    }
}
