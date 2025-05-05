using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddInvoicesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    SystemFee = table.Column<decimal>(type: "numeric", nullable: false),
                    Tax = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentProofUrl = table.Column<string>(type: "text", nullable: true),
                    PaymentSubmittedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PaymentApprovedBy = table.Column<string>(type: "text", nullable: true),
                    PaymentApprovedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PaymentNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_CargoOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "CargoOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                table: "Invoices",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
