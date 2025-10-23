using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddDispatcherFunctionality : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bid_CargoOrders_OrderId",
                table: "Bid");

            migrationBuilder.DropForeignKey(
                name: "FK_Bid_Trucks_TruckId",
                table: "Bid");

            migrationBuilder.DropForeignKey(
                name: "FK_CargoOrders_Bid_AcceptedBidId",
                table: "CargoOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bid",
                table: "Bid");

            migrationBuilder.RenameTable(
                name: "Bid",
                newName: "Bids");

            migrationBuilder.RenameIndex(
                name: "IX_Bid_TruckId",
                table: "Bids",
                newName: "IX_Bids_TruckId");

            migrationBuilder.RenameIndex(
                name: "IX_Bid_OrderId",
                table: "Bids",
                newName: "IX_Bids_OrderId");

            migrationBuilder.AddColumn<bool>(
                name: "CanBidOnBehalf",
                table: "TruckOwners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TruckOwners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "TruckOwners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ManagedByDispatcherId",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnershipType",
                table: "Drivers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DispatcherCommissionAmount",
                table: "Bids",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Bids",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedByDispatcherId",
                table: "Bids",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmitterType",
                table: "Bids",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bids",
                table: "Bids",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DriverDispatcherCommissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriverId = table.Column<string>(type: "text", nullable: false),
                    DispatcherId = table.Column<string>(type: "text", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverDispatcherCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverDispatcherCommissions_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverDispatcherCommissions_TruckOwners_DispatcherId",
                        column: x => x.DispatcherId,
                        principalTable: "TruckOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ManagedByDispatcherId",
                table: "Drivers",
                column: "ManagedByDispatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_SubmittedByDispatcherId",
                table: "Bids",
                column: "SubmittedByDispatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDispatcherCommission_ActiveUnique",
                table: "DriverDispatcherCommissions",
                columns: new[] { "DriverId", "DispatcherId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDispatcherCommissions_DispatcherId",
                table: "DriverDispatcherCommissions",
                column: "DispatcherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_CargoOrders_OrderId",
                table: "Bids",
                column: "OrderId",
                principalTable: "CargoOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_TruckOwners_SubmittedByDispatcherId",
                table: "Bids",
                column: "SubmittedByDispatcherId",
                principalTable: "TruckOwners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Trucks_TruckId",
                table: "Bids",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CargoOrders_Bids_AcceptedBidId",
                table: "CargoOrders",
                column: "AcceptedBidId",
                principalTable: "Bids",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_TruckOwners_ManagedByDispatcherId",
                table: "Drivers",
                column: "ManagedByDispatcherId",
                principalTable: "TruckOwners",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_CargoOrders_OrderId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_TruckOwners_SubmittedByDispatcherId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Trucks_TruckId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_CargoOrders_Bids_AcceptedBidId",
                table: "CargoOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_TruckOwners_ManagedByDispatcherId",
                table: "Drivers");

            migrationBuilder.DropTable(
                name: "DriverDispatcherCommissions");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_ManagedByDispatcherId",
                table: "Drivers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bids",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_SubmittedByDispatcherId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "CanBidOnBehalf",
                table: "TruckOwners");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "TruckOwners");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "TruckOwners");

            migrationBuilder.DropColumn(
                name: "ManagedByDispatcherId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "OwnershipType",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DispatcherCommissionAmount",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "SubmittedByDispatcherId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "SubmitterType",
                table: "Bids");

            migrationBuilder.RenameTable(
                name: "Bids",
                newName: "Bid");

            migrationBuilder.RenameIndex(
                name: "IX_Bids_TruckId",
                table: "Bid",
                newName: "IX_Bid_TruckId");

            migrationBuilder.RenameIndex(
                name: "IX_Bids_OrderId",
                table: "Bid",
                newName: "IX_Bid_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bid",
                table: "Bid",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bid_CargoOrders_OrderId",
                table: "Bid",
                column: "OrderId",
                principalTable: "CargoOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bid_Trucks_TruckId",
                table: "Bid",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CargoOrders_Bid_AcceptedBidId",
                table: "CargoOrders",
                column: "AcceptedBidId",
                principalTable: "Bid",
                principalColumn: "Id");
        }
    }
}
