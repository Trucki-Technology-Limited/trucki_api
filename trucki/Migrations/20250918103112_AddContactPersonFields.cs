using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddContactPersonFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryContactName",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryContactPhone",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupContactName",
                table: "CargoOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupContactPhone",
                table: "CargoOrders",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryContactName",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryContactPhone",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PickupContactName",
                table: "CargoOrders");

            migrationBuilder.DropColumn(
                name: "PickupContactPhone",
                table: "CargoOrders");
        }
    }
}
