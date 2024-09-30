using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddListOfDriversToTruckOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProfilePictureUrl",
                table: "TruckOwners",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IdCardUrl",
                table: "TruckOwners",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "TruckOwnerId",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TruckOwnerId",
                table: "Drivers",
                column: "TruckOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_TruckOwners_TruckOwnerId",
                table: "Drivers",
                column: "TruckOwnerId",
                principalTable: "TruckOwners",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_TruckOwners_TruckOwnerId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TruckOwnerId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "TruckOwnerId",
                table: "Drivers");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePictureUrl",
                table: "TruckOwners",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdCardUrl",
                table: "TruckOwners",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
