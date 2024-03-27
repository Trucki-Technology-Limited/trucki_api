using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class updated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "Managers");

            migrationBuilder.RenameColumn(
                name: "Company",
                table: "Drivers",
                newName: "TruckId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Managers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PassportFile",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DriversLicence",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_ManagerId",
                table: "Businesses",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Managers_ManagerId",
                table: "Businesses",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Managers_ManagerId",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_ManagerId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Businesses");

            migrationBuilder.RenameColumn(
                name: "TruckId",
                table: "Drivers",
                newName: "Company");

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Managers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PassportFile",
                table: "Drivers",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DriversLicence",
                table: "Drivers",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
