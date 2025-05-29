using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddOrderCancellationSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderCancellations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    CargoOwnerId = table.Column<string>(type: "text", nullable: false),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    OrderStatusAtCancellation = table.Column<int>(type: "integer", nullable: false),
                    PenaltyPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalPaymentMethod = table.Column<int>(type: "integer", nullable: true),
                    RefundMethod = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RefundProcessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    IsDriverNotified = table.Column<bool>(type: "boolean", nullable: false),
                    DriverId = table.Column<string>(type: "text", nullable: true),
                    RefundTransactionId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderCancellations_CargoOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "CargoOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderCancellations_CargoOwners_CargoOwnerId",
                        column: x => x.CargoOwnerId,
                        principalTable: "CargoOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderCancellations_CargoOwnerId",
                table: "OrderCancellations",
                column: "CargoOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCancellations_OrderId",
                table: "OrderCancellations",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderCancellations");
        }
    }
}
