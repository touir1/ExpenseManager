using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Expenses.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_FamilyInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FamilyId = table.Column<int>(type: "integer", nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Token = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    InvitedById = table.Column<int>(type: "integer", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyInvitations_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyInvitations_USR_Users_AcceptedByUserId",
                        column: x => x.AcceptedByUserId,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyInvitations_USR_Users_InvitedById",
                        column: x => x.InvitedById,
                        principalSchema: "ext",
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyInvitations_AcceptedByUserId",
                table: "FamilyInvitations",
                column: "AcceptedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyInvitations_FamilyId_InviteeEmail",
                table: "FamilyInvitations",
                columns: new[] { "FamilyId", "InviteeEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyInvitations_InvitedById",
                table: "FamilyInvitations",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyInvitations_Token",
                table: "FamilyInvitations",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyInvitations");
        }
    }
}
