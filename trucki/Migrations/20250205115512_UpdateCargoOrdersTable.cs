using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class UpdateCargoOrdersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PictureOfCargo",
                table: "CargoOrders");

            migrationBuilder.AddColumn<List<string>>(
                name: "CargoImages",
                table: "CargoOrders",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredTruckType",
                table: "CargoOrders",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoImages",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "RequiredTruckType",
                table: "CargoOrders");

            migrationBuilder.AddColumn<string>(
                name: "PictureOfCargo",
                table: "CargoOrders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
