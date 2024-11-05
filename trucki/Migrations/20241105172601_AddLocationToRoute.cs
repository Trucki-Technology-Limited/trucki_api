using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddLocationToRoute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromRouteLat",
                table: "RoutesEnumerable",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromRouteLng",
                table: "RoutesEnumerable",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToRouteLat",
                table: "RoutesEnumerable",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToRouteLng",
                table: "RoutesEnumerable",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromRouteLat",
                table: "RoutesEnumerable");

            migrationBuilder.DropColumn(
                name: "FromRouteLng",
                table: "RoutesEnumerable");

            migrationBuilder.DropColumn(
                name: "ToRouteLat",
                table: "RoutesEnumerable");

            migrationBuilder.DropColumn(
                name: "ToRouteLng",
                table: "RoutesEnumerable");
        }
    }
}
