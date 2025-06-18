using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class StripeConnect : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanReceivePayouts",
                table: "Drivers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StripeAccountCreatedAt",
                table: "Drivers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StripeAccountStatus",
                table: "Drivers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripeConnectAccountId",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverEarnings",
                table: "CargoOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagReason",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagResolutionNotes",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FlagResolvedAt",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagResolvedBy",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FlaggedAt",
                table: "CargoOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlaggedBy",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlagged",
                table: "CargoOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DriverPayouts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriverId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StripeTransferId = table.Column<string>(type: "text", nullable: true),
                    StripePayoutId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    OrdersCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverPayouts_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverPayoutOrder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriverPayoutId = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    OrderEarnings = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderCompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPayoutOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverPayoutOrder_CargoOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "CargoOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverPayoutOrder_DriverPayouts_DriverPayoutId",
                        column: x => x.DriverPayoutId,
                        principalTable: "DriverPayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPayoutOrder_DriverPayoutId",
                table: "DriverPayoutOrder",
                column: "DriverPayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPayoutOrder_OrderId",
                table: "DriverPayoutOrder",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPayouts_DriverId",
                table: "DriverPayouts",
                column: "DriverId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverPayoutOrder");

            migrationBuilder.DropTable(
                name: "DriverPayouts");

            migrationBuilder.DropColumn(
                name: "CanReceivePayouts",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "StripeAccountCreatedAt",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "StripeAccountStatus",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "StripeConnectAccountId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DriverEarnings",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "FlagReason",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "FlagResolutionNotes",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "FlagResolvedAt",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "FlagResolvedBy",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "FlaggedAt",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "FlaggedBy",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "IsFlagged",
                table: "CargoOrders");
        }
    }
}
