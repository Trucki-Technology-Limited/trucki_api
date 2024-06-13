using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class AddBankDetailsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankDetailsId",
                table: "TruckOwners",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: false),
                    BankAccountName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TruckOwners_BankDetailsId",
                table: "TruckOwners",
                column: "BankDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckOwners_BankDetails_BankDetailsId",
                table: "TruckOwners",
                column: "BankDetailsId",
                principalTable: "BankDetails",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckOwners_BankDetails_BankDetailsId",
                table: "TruckOwners");

            migrationBuilder.DropTable(
                name: "BankDetails");

            migrationBuilder.DropIndex(
                name: "IX_TruckOwners_BankDetailsId",
                table: "TruckOwners");

            migrationBuilder.DropColumn(
                name: "BankDetailsId",
                table: "TruckOwners");
        }
    }
}
