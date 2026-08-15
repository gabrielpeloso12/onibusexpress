using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OniBusExpress.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJwtAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_UserName",
                table: "AdminUsers",
                column: "UserName",
                unique: true);
        }
    }
}
