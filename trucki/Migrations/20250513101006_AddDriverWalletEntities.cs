using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddDriverWalletEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverWallets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriverId = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    PendingWithdrawal = table.Column<decimal>(type: "numeric", nullable: false),
                    NextWithdrawal = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverWallets_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverWithdrawalSchedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactionsCount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverWithdrawalSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverWalletTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    WalletId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RelatedOrderId = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    BankTransferReference = table.Column<string>(type: "text", nullable: true),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
                    DriverWithdrawalScheduleId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverWalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverWalletTransactions_CargoOrders_RelatedOrderId",
                        column: x => x.RelatedOrderId,
                        principalTable: "CargoOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DriverWalletTransactions_DriverWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "DriverWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverWalletTransactions_DriverWithdrawalSchedules_DriverWi~",
                        column: x => x.DriverWithdrawalScheduleId,
                        principalTable: "DriverWithdrawalSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverWallets_DriverId",
                table: "DriverWallets",
                column: "DriverId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverWalletTransactions_DriverWithdrawalScheduleId",
                table: "DriverWalletTransactions",
                column: "DriverWithdrawalScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverWalletTransactions_RelatedOrderId",
                table: "DriverWalletTransactions",
                column: "RelatedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverWalletTransactions_WalletId",
                table: "DriverWalletTransactions",
                column: "WalletId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverWalletTransactions");

            migrationBuilder.DropTable(
                name: "DriverWallets");

            migrationBuilder.DropTable(
                name: "DriverWithdrawalSchedules");
        }
    }
}
