using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddDotMcNumbersToTruckOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DotNumber",
                table: "TruckOwners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "McNumber",
                table: "TruckOwners",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DotNumber",
                table: "TruckOwners");

            migrationBuilder.DropColumn(
                name: "McNumber",
                table: "TruckOwners");
        }
    }
}
