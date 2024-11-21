using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddUserIdToOfficer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Officers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Officers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Officers_UserId",
                table: "Officers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Officers_AspNetUsers_UserId",
                table: "Officers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Officers_AspNetUsers_UserId",
                table: "Officers");

            migrationBuilder.DropIndex(
                name: "IX_Officers_UserId",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Officers");
        }
    }
}
