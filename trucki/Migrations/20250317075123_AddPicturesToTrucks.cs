using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddPicturesToTrucks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CargoSpacePictureUrl",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalTruckPictureUrl",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDriverOwnedTruck",
                table: "Trucks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoSpacePictureUrl",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "ExternalTruckPictureUrl",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "IsDriverOwnedTruck",
                table: "Trucks");
        }
    }
}
