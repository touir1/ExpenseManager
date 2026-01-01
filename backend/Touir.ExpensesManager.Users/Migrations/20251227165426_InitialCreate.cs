using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Touir.ExpensesManager.Users.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_Applications",
                columns: table => new
                {
                    APP_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    APP_Code = table.Column<string>(type: "text", nullable: false),
                    APP_Name = table.Column<string>(type: "text", nullable: false),
                    APP_Description = table.Column<string>(type: "text", nullable: true),
                    APP_UrlPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_Applications", x => x.APP_Id);
                });

            migrationBuilder.CreateTable(
                name: "USR_Users",
                columns: table => new
                {
                    USR_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    USR_FirstName = table.Column<string>(type: "text", nullable: false),
                    USR_LastName = table.Column<string>(type: "text", nullable: false),
                    USR_Email = table.Column<string>(type: "text", nullable: false),
                    USR_FamilyId = table.Column<int>(type: "integer", nullable: true),
                    USR_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    USR_CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    USR_LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    USR_LastUpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    USR_IsEmailValidated = table.Column<bool>(type: "boolean", nullable: false),
                    USR_EmailValidationHash = table.Column<string>(type: "text", nullable: true),
                    USR_IsDisabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USR_Users", x => x.USR_Id);
                    table.ForeignKey(
                        name: "FK_USR_Users_USR_Users_USR_CreatedBy",
                        column: x => x.USR_CreatedBy,
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USR_Users_USR_Users_USR_LastUpdatedBy",
                        column: x => x.USR_LastUpdatedBy,
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RLE_Roles",
                columns: table => new
                {
                    RLE_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    RLE_Code = table.Column<string>(type: "text", nullable: false),
                    RLE_Name = table.Column<string>(type: "text", nullable: false),
                    RLE_Description = table.Column<string>(type: "text", nullable: true),
                    RLE_ApplicationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RLE_Roles", x => x.RLE_Id);
                    table.ForeignKey(
                        name: "FK_RLE_Roles_APP_Applications_RLE_ApplicationId",
                        column: x => x.RLE_ApplicationId,
                        principalTable: "APP_Applications",
                        principalColumn: "APP_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RQA_RequestAccesses",
                columns: table => new
                {
                    RQA_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    RQA_Name = table.Column<string>(type: "text", nullable: false),
                    RQA_Description = table.Column<string>(type: "text", nullable: true),
                    RQA_Path = table.Column<string>(type: "text", nullable: false),
                    RQA_Type = table.Column<string>(type: "text", nullable: false),
                    RQA_IsAuthenticationNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    RQA_ApplicationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RQA_RequestAccesses", x => x.RQA_Id);
                    table.ForeignKey(
                        name: "FK_RQA_RequestAccesses_APP_Applications_RQA_ApplicationId",
                        column: x => x.RQA_ApplicationId,
                        principalTable: "APP_Applications",
                        principalColumn: "APP_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ATH_Authentications",
                columns: table => new
                {
                    ATH_UserId = table.Column<int>(type: "integer", nullable: false),
                    ATH_HashPassword = table.Column<string>(type: "text", nullable: false),
                    ATH_HashSalt = table.Column<string>(type: "text", nullable: false),
                    ATH_IsTemporaryPassword = table.Column<bool>(type: "boolean", nullable: false),
                    ATH_PasswordResetHash = table.Column<string>(type: "text", nullable: true),
                    ATH_PasswordResetRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATH_Authentications", x => x.ATH_UserId);
                    table.ForeignKey(
                        name: "FK_ATH_Authentications_USR_Users_ATH_UserId",
                        column: x => x.ATH_UserId,
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "URR_UserRoles",
                columns: table => new
                {
                    URR_UserId = table.Column<int>(type: "integer", nullable: false),
                    URR_RoleId = table.Column<int>(type: "integer", nullable: false),
                    URR_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    URR_CreatedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_URR_UserRoles", x => new { x.URR_UserId, x.URR_RoleId });
                    table.ForeignKey(
                        name: "FK_URR_UserRoles_RLE_Roles_URR_RoleId",
                        column: x => x.URR_RoleId,
                        principalTable: "RLE_Roles",
                        principalColumn: "RLE_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_URR_UserRoles_USR_Users_URR_CreatedById",
                        column: x => x.URR_CreatedById,
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_URR_UserRoles_USR_Users_URR_UserId",
                        column: x => x.URR_UserId,
                        principalTable: "USR_Users",
                        principalColumn: "USR_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RRA_RoleRequestAccesses",
                columns: table => new
                {
                    RRA_RoleId = table.Column<int>(type: "integer", nullable: false),
                    RRA_RequestAccessId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RRA_RoleRequestAccesses", x => new { x.RRA_RoleId, x.RRA_RequestAccessId });
                    table.ForeignKey(
                        name: "FK_RRA_RoleRequestAccesses_RLE_Roles_RRA_RoleId",
                        column: x => x.RRA_RoleId,
                        principalTable: "RLE_Roles",
                        principalColumn: "RLE_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RRA_RoleRequestAccesses_RQA_RequestAccesses_RRA_RequestAcce~",
                        column: x => x.RRA_RequestAccessId,
                        principalTable: "RQA_RequestAccesses",
                        principalColumn: "RQA_Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_Applications_APP_Code",
                table: "APP_Applications",
                column: "APP_Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RLE_Roles_RLE_ApplicationId",
                table: "RLE_Roles",
                column: "RLE_ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RQA_RequestAccesses_RQA_ApplicationId",
                table: "RQA_RequestAccesses",
                column: "RQA_ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RRA_RoleRequestAccesses_RRA_RequestAccessId",
                table: "RRA_RoleRequestAccesses",
                column: "RRA_RequestAccessId");

            migrationBuilder.CreateIndex(
                name: "IX_URR_UserRoles_URR_CreatedById",
                table: "URR_UserRoles",
                column: "URR_CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_URR_UserRoles_URR_RoleId",
                table: "URR_UserRoles",
                column: "URR_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_USR_Users_USR_CreatedBy",
                table: "USR_Users",
                column: "USR_CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_USR_Users_USR_LastUpdatedBy",
                table: "USR_Users",
                column: "USR_LastUpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ATH_Authentications");

            migrationBuilder.DropTable(
                name: "RRA_RoleRequestAccesses");

            migrationBuilder.DropTable(
                name: "URR_UserRoles");

            migrationBuilder.DropTable(
                name: "RQA_RequestAccesses");

            migrationBuilder.DropTable(
                name: "RLE_Roles");

            migrationBuilder.DropTable(
                name: "USR_Users");

            migrationBuilder.DropTable(
                name: "APP_Applications");
        }
    }
}
