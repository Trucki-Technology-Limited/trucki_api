using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddNewCargoOrderFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CargoOwners",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoOwners_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bid",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    truckId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bid_Trucks_truckId",
                        column: x => x.truckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargoOrders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CargoOwnerId = table.Column<string>(type: "text", nullable: false),
                    PickupLocation = table.Column<string>(type: "text", nullable: false),
                    PickupLocationLat = table.Column<string>(type: "text", nullable: false),
                    PickupLocationLong = table.Column<string>(type: "text", nullable: false),
                    DeliveryLocation = table.Column<string>(type: "text", nullable: false),
                    DeliveryLocationLat = table.Column<string>(type: "text", nullable: false),
                    DeliveryLocationLong = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    AcceptedBidId = table.Column<string>(type: "text", nullable: true),
                    Documents = table.Column<string>(type: "text", nullable: true),
                    DeliveryDocuments = table.Column<string>(type: "text", nullable: true),
                    Consignment = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<string>(type: "text", nullable: false),
                    CargoType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    PictureOfCargo = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoOrders_Bid_AcceptedBidId",
                        column: x => x.AcceptedBidId,
                        principalTable: "Bid",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CargoOrders_CargoOwners_CargoOwnerId",
                        column: x => x.CargoOwnerId,
                        principalTable: "CargoOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bid_OrderId",
                table: "Bid",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Bid_truckId",
                table: "Bid",
                column: "truckId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoOrders_AcceptedBidId",
                table: "CargoOrders",
                column: "AcceptedBidId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargoOrders_CargoOwnerId",
                table: "CargoOrders",
                column: "CargoOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoOwners_UserId",
                table: "CargoOwners",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bid_CargoOrders_OrderId",
                table: "Bid",
                column: "OrderId",
                principalTable: "CargoOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bid_CargoOrders_OrderId",
                table: "Bid");

            migrationBuilder.DropTable(
                name: "CargoOrders");

            migrationBuilder.DropTable(
                name: "Bid");

            migrationBuilder.DropTable(
                name: "CargoOwners");
        }
    }
}
