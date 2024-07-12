using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class UpdateTruckAndDriverModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TruckId",
                table: "Drivers",
                column: "TruckId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Trucks_TruckId",
                table: "Drivers",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Trucks_TruckId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TruckId",
                table: "Drivers");
        }
    }
}
