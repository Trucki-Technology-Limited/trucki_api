using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class NewModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Company",
                table: "Managers",
                newName: "CompanyId");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Managers_CompanyId",
                table: "Managers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CompanyId",
                table: "Drivers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Company_CompanyId",
                table: "Drivers",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Company_CompanyId",
                table: "Managers",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Company_CompanyId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Company_CompanyId",
                table: "Managers");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Managers_CompanyId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CompanyId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Managers",
                newName: "Company");
        }
    }
}
