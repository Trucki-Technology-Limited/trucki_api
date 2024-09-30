using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddUserEntityToTruckOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TruckOwners",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruckOwners_UserId",
                table: "TruckOwners",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckOwners_AspNetUsers_UserId",
                table: "TruckOwners",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckOwners_AspNetUsers_UserId",
                table: "TruckOwners");

            migrationBuilder.DropIndex(
                name: "IX_TruckOwners_UserId",
                table: "TruckOwners");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TruckOwners");
        }
    }
}
