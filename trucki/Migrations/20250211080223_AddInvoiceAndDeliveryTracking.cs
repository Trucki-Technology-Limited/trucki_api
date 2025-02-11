using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddInvoiceAndDeliveryTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bid_Trucks_truckId",
                table: "Bid");

            migrationBuilder.DropColumn(
                name: "CargoImages",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "CargoType",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CargoOrders");

            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "CargoOrders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "truckId",
                table: "Bid",
                newName: "TruckId");

            migrationBuilder.RenameIndex(
                name: "IX_Bid_truckId",
                table: "Bid",
                newName: "IX_Bid_TruckId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualPickupDateTime",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDateTime",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueDate",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "CargoOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupDateTime",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SystemFee",
                table: "CargoOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "CargoOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DriverAcknowledgedAt",
                table: "Bid",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CargoOrderItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CargoOrderId = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Length = table.Column<decimal>(type: "numeric", nullable: false),
                    Width = table.Column<decimal>(type: "numeric", nullable: false),
                    Height = table.Column<decimal>(type: "numeric", nullable: false),
                    IsFragile = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialHandlingInstructions = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ItemImages = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoOrderItem_CargoOrders_CargoOrderId",
                        column: x => x.CargoOrderId,
                        principalTable: "CargoOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoOrderItem_CargoOrderId",
                table: "CargoOrderItem",
                column: "CargoOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bid_Trucks_TruckId",
                table: "Bid",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bid_Trucks_TruckId",
                table: "Bid");

            migrationBuilder.DropTable(
                name: "CargoOrderItem");

            migrationBuilder.DropColumn(
                name: "ActualPickupDateTime",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryDateTime",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PaymentDueDate",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PickupDateTime",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "SystemFee",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "DriverAcknowledgedAt",
                table: "Bid");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "CargoOrders",
                newName: "Weight");

            migrationBuilder.RenameColumn(
                name: "TruckId",
                table: "Bid",
                newName: "truckId");

            migrationBuilder.RenameIndex(
                name: "IX_Bid_TruckId",
                table: "Bid",
                newName: "IX_Bid_truckId");

            migrationBuilder.AddColumn<List<string>>(
                name: "CargoImages",
                table: "CargoOrders",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "CargoType",
                table: "CargoOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CargoOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Quantity",
                table: "CargoOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Bid_Trucks_truckId",
                table: "Bid",
                column: "truckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
