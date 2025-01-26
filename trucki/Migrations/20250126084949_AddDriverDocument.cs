using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddDriverDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "NG");

            migrationBuilder.CreateTable(
                name: "DriverDocument",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriverId = table.Column<string>(type: "text", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "integer", nullable: false),
                    DocumentTypeId1 = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverDocument_DocumentTypes_DocumentTypeId1",
                        column: x => x.DocumentTypeId1,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverDocument_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocument_DocumentTypeId1",
                table: "DriverDocument",
                column: "DocumentTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocument_DriverId",
                table: "DriverDocument",
                column: "DriverId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverDocument");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Drivers");
        }
    }
}
