using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxEvents",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxEvents", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxEvents_ReceivedAt",
                table: "InboxEvents",
                column: "ReceivedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxEvents");
        }
    }
}
