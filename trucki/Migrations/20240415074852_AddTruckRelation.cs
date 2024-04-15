using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddTruckRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TruckOwnerId",
                table: "Trucks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TruckId",
                table: "Drivers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_TruckOwnerId",
                table: "Trucks",
                column: "TruckOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_TruckOwners_TruckOwnerId",
                table: "Trucks",
                column: "TruckOwnerId",
                principalTable: "TruckOwners",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_TruckOwners_TruckOwnerId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_TruckOwnerId",
                table: "Trucks");

            migrationBuilder.AlterColumn<string>(
                name: "TruckOwnerId",
                table: "Trucks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TruckId",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
