using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class updateTruckmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Drivers_DriverId1",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId1",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "DriverId1",
                table: "Trucks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverId1",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId1",
                table: "Trucks",
                column: "DriverId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Drivers_DriverId1",
                table: "Trucks",
                column: "DriverId1",
                principalTable: "Drivers",
                principalColumn: "Id");
        }
    }
}
