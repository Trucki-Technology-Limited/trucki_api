using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddCargoOwnerWallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StripePaymentAmount",
                table: "CargoOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WalletPaymentAmount",
                table: "CargoOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CargoOwnerWallets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CargoOwnerId = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoOwnerWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoOwnerWallets_CargoOwners_CargoOwnerId",
                        column: x => x.CargoOwnerId,
                        principalTable: "CargoOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    WalletId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RelatedOrderId = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_CargoOrders_RelatedOrderId",
                        column: x => x.RelatedOrderId,
                        principalTable: "CargoOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_CargoOwnerWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "CargoOwnerWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoOwnerWallets_CargoOwnerId",
                table: "CargoOwnerWallets",
                column: "CargoOwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_RelatedOrderId",
                table: "WalletTransactions",
                column: "RelatedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "CargoOwnerWallets");

            migrationBuilder.DropColumn(
                name: "StripePaymentAmount",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "WalletPaymentAmount",
                table: "CargoOrders");
        }
    }
}
