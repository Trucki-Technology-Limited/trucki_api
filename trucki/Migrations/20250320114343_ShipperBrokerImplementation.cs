using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class ShipperBrokerImplementation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "CargoOwners",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "CargoOwners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermDays",
                table: "CargoOwners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "CargoOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "CargoOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "CargoOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "CargoOwners");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "CargoOwners");

            migrationBuilder.DropColumn(
                name: "PaymentTermDays",
                table: "CargoOwners");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "CargoOrders");
        }
    }
}
